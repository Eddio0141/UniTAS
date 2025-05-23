using System;
using System.Collections.Generic;
using UniTAS.Patcher.Implementations.Coroutine;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Interfaces.GlobalHotkeyListener;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Customization;
using UniTAS.Patcher.Services.FrameAdvancing;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;
using UnityEngine;


namespace UniTAS.Patcher.Implementations.FrameAdvancing;

// this class needs to run before coroutine is processed in CoroutineHandler (for tracking fixed update index)
// also needs to run before SyncFixedUpdateCycle to process sync method invoke, then handling new frame advancing stuff
[Singleton(RegisterPriority.FrameAdvancing)]
[ExcludeRegisterIfTesting]
public partial class FrameAdvancing : IFrameAdvancing, IOnFixedUpdateUnconditional, IOnGameRestartResume,
    IOnUpdateUnconditional, IOnLateUpdateUnconditional
{
    private bool _active;

    private readonly Queue<PendingFrameAdvance> _pendingFrameAdvances = new();
    private uint _pendingPauseFrames;

    // for after fixed update and next update isn't in sync
    private PendingUpdateOffsetFixState _pendingUpdateOffsetFixState = PendingUpdateOffsetFixState.Done;

    // offset to compare with Update
    private double _pendingUpdateOffsetFixStateCheckingOffset;

    private readonly Action _updateOffsetSyncFix;

    private bool _paused;
    private bool _pendingPause;

    private double _updateRestoreOffset;
    private uint _fixedUpdateRestoreIndex;
    private uint _fixedUpdateIndex;
    private bool _updated;
    private bool _pendingUnpause;

    private readonly Action _unpauseActual;

    private readonly IMonoBehaviourController _monoBehaviourController;
    private readonly ISyncFixedUpdateCycle _syncFixedUpdate;
    private readonly ICoroutine _coroutine;
    private readonly ITimeEnv _timeEnv;
    private readonly ILogger _logger;
    private readonly IUpdateInvokeOffset _updateInvokeOffset;

    public FrameAdvancing(IMonoBehaviourController monoBehaviourController, IBinds binds,
        ISyncFixedUpdateCycle syncFixedUpdate, ICoroutine coroutine, ITimeEnv timeEnv, ILogger logger,
        IGlobalHotkey globalHotkey, IUpdateInvokeOffset updateInvokeOffset)
    {
        // var frameAdvanceBind = binds.Create(new("FrameAdvance", KeyCode.Slash));
        // var frameAdvanceToggleBind = binds.Create(new("FrameAdvanceToggle", KeyCode.Period));
        // TODO this needs to be customizable
        // globalHotkey.AddGlobalHotkey(new(frameAdvanceBind, () => FrameAdvance(1, FrameAdvanceMode.Update)));
        // globalHotkey.AddGlobalHotkey(new(frameAdvanceToggleBind, TogglePause));

        _monoBehaviourController = monoBehaviourController;
        _syncFixedUpdate = syncFixedUpdate;
        _coroutine = coroutine;
        _timeEnv = timeEnv;
        _logger = logger;
        _updateInvokeOffset = updateInvokeOffset;
        _unpauseActual = UnpauseActual;
        _updateOffsetSyncFix = UpdateOffsetSyncFix;
    }

    public void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        if (!preMonoBehaviourResume) return;
        _fixedUpdateIndex = 0;
        _updated = false;
        _active = false;
    }

    public void FrameAdvance(uint frames)
    {
        if (!_active)
        {
            // ok frame advancing needs to be activated
            _active = true;
            frames = 0;
        }

        _pendingFrameAdvances.Enqueue(new(frames));
        _logger.LogDebug($"added frame advance queue, count: {frames}");
    }

    public void TogglePause()
    {
        _active = !_active;
        _logger.LogDebug($"toggled frame advance, active: {_active}");
    }

    public void UpdateUnconditional()
    {
        if (_updated) return;
        _updated = true;

        _fixedUpdateIndex = 0;

        // check if update offset is valid after the last fixed update
        if (_pendingUpdateOffsetFixState == PendingUpdateOffsetFixState.PendingCheckUpdateOffset)
        {
            var actualOffset = _pendingUpdateOffsetFixStateCheckingOffset + _timeEnv.FrameTime;

            // is it invalid
            if (Math.Abs(actualOffset - _updateInvokeOffset.Offset) % Time.fixedDeltaTime > _timeEnv.TimeTolerance)
            {
                _logger.LogDebug(
                    $"invalid offset after FixedUpdate, expected {actualOffset}, current: {_updateInvokeOffset.Offset}, fixing");
                _syncFixedUpdate.OnSync(_updateOffsetSyncFix, actualOffset);
                // pause until offset is synced
                // this also prevents this broken Update
                _monoBehaviourController.PausedExecution = true;
                _pendingUpdateOffsetFixState = PendingUpdateOffsetFixState.PendingSync;

                return;
            }

            // not invalid, continue
            _pendingUpdateOffsetFixState = PendingUpdateOffsetFixState.Done;
        }

        FrameAdvanceUpdate();
    }

    public void OnLateUpdateUnconditional()
    {
        _updated = false;
    }

    public void FixedUpdateUnconditional()
    {
        _fixedUpdateIndex++;
    }

    private void FrameAdvanceUpdate()
    {
        if (_pendingUpdateOffsetFixState is not (PendingUpdateOffsetFixState.Done
            or PendingUpdateOffsetFixState.PendingCheckUpdateOffset))
        {
            StaticLogger.Trace("waiting for update offset fix");
            return;
        }

        // if unpause -> pause, it needs to wait for unpause operation to finish since that takes time
        if (_pendingUnpause) return;

        if (!_active)
        {
            // resume game
            Unpause();
            return;
        }

        // wait for frames until pause
        if (_pendingPauseFrames > 0)
        {
            // skip 1 frame when pause
            if (!_paused)
            {
                _pendingPauseFrames--;
            }

            Unpause();

            CheckAndAddPendingFrameAdvances();

            // if not instant callback, return
            if (_pendingUnpause) return;
        }

        CheckAndAddPendingFrameAdvances();

        // do we have new frame advance to do? don't bother pausing then
        if (_pendingPauseFrames != 0) return;

        _coroutine.Start(Pause()).OnComplete += status =>
        {
            if (status.Exception != null)
                _logger.LogFatal($"exception occurs during frame advance coroutine: {status.Exception}");
        };
    }

    private void CheckAndAddPendingFrameAdvances()
    {
        if (_pendingFrameAdvances.Count <= 0) return;

        var pendingFrameAdvance = _pendingFrameAdvances.Dequeue();

        // add queued frames
        _pendingPauseFrames += pendingFrameAdvance.PendingFrames;

        _logger.LogDebug($"adding pending frame advances, pending pause frames: {_pendingPauseFrames}");
    }

    private IEnumerable<CoroutineWait> Pause()
    {
        if (_paused || _pendingPause) yield break;
        _pendingPause = true;

        // pause at right timing, basically let the game run until reached requirement timing of pause depending on user choice
        _logger.LogDebug("Pausing frame advance on Update");
        yield return new WaitForUpdateActual();

        PauseActual();
        _pendingPause = false;
    }

    private void PauseActual()
    {
        _updateRestoreOffset = _updateInvokeOffset.Offset;
        _fixedUpdateRestoreIndex = _fixedUpdateIndex;
        _monoBehaviourController.PausedExecution = true;
        _logger.LogDebug(
            $"Pause frame advance, restore offset: {_updateRestoreOffset}, fixed update index: {_fixedUpdateRestoreIndex}");

        _paused = true;
    }

    private void Unpause()
    {
        if (!_paused || _pendingUnpause) return;
        _pendingUnpause = true;

        _logger.LogDebug($"unpause, restore time: {_updateRestoreOffset}, restore index: {_fixedUpdateRestoreIndex}");
        _syncFixedUpdate.OnSync(_unpauseActual, _updateRestoreOffset, _fixedUpdateRestoreIndex);
    }

    private void UnpauseActual()
    {
        _monoBehaviourController.PausedExecution = false;
        _paused = false;
        _pendingUnpause = false;

        if (_fixedUpdateRestoreIndex != 0)
        {
            // fixed update, check on next update if offset is broken
            _pendingUpdateOffsetFixState = PendingUpdateOffsetFixState.PendingCheckUpdateOffset;
            _pendingUpdateOffsetFixStateCheckingOffset = _updateRestoreOffset;
        }
    }

    private void UpdateOffsetSyncFix()
    {
        // done fixing offset
        _pendingUpdateOffsetFixState = PendingUpdateOffsetFixState.Done;
        _logger.LogDebug("fixed update offset");
        PauseActual();
    }
}