using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Models.Coroutine;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Customization;
using UniTAS.Patcher.Services.FrameAdvancing;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.FrameAdvancing;

// this class needs to run before coroutine is processed in CoroutineHandler (for tracking fixed update index)
// also needs to run after SyncFixedUpdateCycle to process sync method invoke, then handling new fa stuff
[Singleton(RegisterPriority.FrameAdvancing)]
public class FrameAdvancing : IFrameAdvancing, IOnUpdateUnconditional, IOnFixedUpdateUnconditional
{
    private bool _active;

    private readonly Queue<PendingFrameAdvance> _pendingFrameAdvances = new();
    private uint _pendingPauseFrames;
    private FrameAdvanceMode _frameAdvanceMode;

    private bool _paused;
    private bool _pendingPause;

    private double _updateRestoreOffset;
    private uint _fixedUpdateRestoreIndex;
    private uint _fixedUpdateIndex;
    private bool _pendingUnpause;

    private readonly Action _unpauseActual;

    private readonly IMonoBehaviourController _monoBehaviourController;
    private readonly ISyncFixedUpdateCycle _syncFixedUpdate;
    private readonly ITimeEnv _timeEnv;
    private readonly ICoroutine _coroutine;

    public FrameAdvancing(IMonoBehaviourController monoBehaviourController, IBinds binds,
        ISyncFixedUpdateCycle syncFixedUpdate, ITimeEnv timeEnv, ICoroutine coroutine)
    {
        // TODO clean these binds up
        binds.Create(new("FrameAdvance", KeyCode.Slash));
        binds.Create(new("FrameAdvanceResume", KeyCode.Period));

        _monoBehaviourController = monoBehaviourController;
        _syncFixedUpdate = syncFixedUpdate;
        _timeEnv = timeEnv;
        _coroutine = coroutine;
        _unpauseActual = UnpauseActual;
    }

    public void FrameAdvance(uint frames, FrameAdvanceMode frameAdvanceMode)
    {
        if (!_active)
        {
            // ok frame advancing needs to be activated
            _active = true;
            frames = 0;
        }

        _pendingFrameAdvances.Enqueue(new(frames, frameAdvanceMode));
    }

    public void TogglePause()
    {
        _active = !_active;
    }

    public void UpdateUnconditional()
    {
        if (!_monoBehaviourController.PausedExecution && !_monoBehaviourController.PausedUpdate)
        {
            _fixedUpdateIndex = 0;
        }

        FrameAdvanceUpdate(true);
    }

    public void FixedUpdateUnconditional()
    {
        if (!_monoBehaviourController.PausedExecution)
        {
            _fixedUpdateIndex++;
        }

        FrameAdvanceUpdate(false);
    }

    private void FrameAdvanceUpdate(bool update)
    {
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

            return;
        }

        CheckAndAddPendingFrameAdvances();

        _coroutine.Start(Pause(update));
    }

    private void CheckAndAddPendingFrameAdvances()
    {
        // check if we got any more frame advances to be done
        if (_pendingFrameAdvances.Count <= 0) return;

        var pendingFrameAdvance = _pendingFrameAdvances.Dequeue();
        _pendingPauseFrames = pendingFrameAdvance.PendingFrames;
        _frameAdvanceMode = pendingFrameAdvance.FrameAdvanceMode;
    }

    private IEnumerator<CoroutineWait> Pause(bool update)
    {
        if (_paused || _pendingPause) yield break;
        _pendingPause = true;

        // pause at right timing, basically let the game run until reached requirement timing of pause depending on user choice
        switch (update)
        {
            case true when (_frameAdvanceMode & FrameAdvanceMode.Update) == 0:
                // update but pause mode is fixed update only
                yield return new WaitForFixedUpdateUnconditional();
                break;
            case false when (_frameAdvanceMode & FrameAdvanceMode.FixedUpdate) == 0:
                yield return new WaitForUpdateUnconditional();
                break;
        }

        _updateRestoreOffset = UpdateInvokeOffset.Offset;
        _fixedUpdateRestoreIndex = _fixedUpdateIndex;
        _monoBehaviourController.PausedExecution = true;
        // TODO remove this log
        StaticLogger.Log.LogDebug(
            $"Pause frame advance, restore offset: {_updateRestoreOffset}, fixed update index: {_fixedUpdateRestoreIndex}");

        _paused = true;
        _pendingPause = false;
    }

    private void Unpause()
    {
        if (!_paused || _pendingUnpause) return;
        _pendingUnpause = true;

        if (_fixedUpdateRestoreIndex != 0)
        {
            _syncFixedUpdate.OnSync(_unpauseActual, _updateRestoreOffset, _fixedUpdateRestoreIndex);
        }
        else
        {
            _syncFixedUpdate.OnSync(_unpauseActual, _updateRestoreOffset + _timeEnv.FrameTime);
        }
    }

    private void UnpauseActual()
    {
        _monoBehaviourController.PausedExecution = false;
        _paused = false;
        _pendingUnpause = false;
    }

    private struct PendingFrameAdvance
    {
        public readonly uint PendingFrames;
        public readonly FrameAdvanceMode FrameAdvanceMode;

        public PendingFrameAdvance(uint pendingFrames, FrameAdvanceMode frameAdvanceMode)
        {
            PendingFrames = pendingFrames;
            FrameAdvanceMode = frameAdvanceMode;
        }
    }
}

[Flags]
public enum FrameAdvanceMode
{
    Update = 1,
    FixedUpdate = 2
}