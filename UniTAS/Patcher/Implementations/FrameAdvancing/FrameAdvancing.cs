using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Customization;
using UniTAS.Patcher.Services.FrameAdvancing;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.FrameAdvancing;

[Singleton(RegisterPriority.FrameAdvancing)]
public class FrameAdvancing : IFrameAdvancing, IOnUpdateUnconditional, IOnFixedUpdateUnconditional
{
    private bool _active;

    private uint _pendingPauseFrames;

    private bool _paused;
    private double _updateRestoreOffset;
    private uint _fixedUpdateRestoreIndex;
    private uint _fixedUpdateIndex;
    private bool _pendingUnpause;

    private readonly Action _unpauseActual;

    private FrameAdvanceMode _frameAdvanceMode = FrameAdvanceMode.Update;

    private readonly IMonoBehaviourController _monoBehaviourController;
    private readonly ISyncFixedUpdateCycle _syncFixedUpdate;
    private readonly ITimeEnv _timeEnv;

    public FrameAdvancing(IMonoBehaviourController monoBehaviourController, IBinds binds,
        ISyncFixedUpdateCycle syncFixedUpdate, ITimeEnv timeEnv)
    {
        // TODO clean these binds up
        binds.Create(new("FrameAdvance", KeyCode.Slash));
        binds.Create(new("FrameAdvanceResume", KeyCode.Period));

        _monoBehaviourController = monoBehaviourController;
        _syncFixedUpdate = syncFixedUpdate;
        _timeEnv = timeEnv;
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

        _pendingPauseFrames = frames;
        _frameAdvanceMode = frameAdvanceMode;
    }

    public void TogglePause()
    {
        _active = !_active;
    }

    // unpause depends on syncing update, and this class needs to run AFTER the sync update
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

            return;
        }

        Pause(update);
    }

    private void Pause(bool update)
    {
        if (_paused) return;
        _paused = true;
        // TODO remove this log
        StaticLogger.Log.LogDebug("Pause frame advance");

        // the pause happens before the updates

        // are we pausing at right timing
        if ((!update || (_frameAdvanceMode & FrameAdvanceMode.Update) == 0) &&
            (update || (_frameAdvanceMode & FrameAdvanceMode.FixedUpdate) == 0)) return;

        _updateRestoreOffset = UpdateInvokeOffset.Offset;
        _fixedUpdateRestoreIndex = _fixedUpdateIndex;
        _monoBehaviourController.PausedExecution = true;
    }

    private void Unpause()
    {
        if (!_paused || _pendingUnpause) return;
        _pendingUnpause = true;
        // TODO also remove this log
        StaticLogger.Log.LogDebug("Unpause frame advance");

        if (IsNextUpdateFixedUpdate())
        {
            _syncFixedUpdate.OnSync(_unpauseActual, _updateRestoreOffset, _fixedUpdateRestoreIndex + 1);
        }
        else
        {
            _syncFixedUpdate.OnSync(_unpauseActual, _updateRestoreOffset + _timeEnv.FrameTime);
        }
    }

    private bool IsNextUpdateFixedUpdate()
    {
        var futureOffset = _updateRestoreOffset + _timeEnv.FrameTime;
        return futureOffset >= (_fixedUpdateRestoreIndex + 1) * Time.fixedDeltaTime;
    }

    private void UnpauseActual()
    {
        _monoBehaviourController.PausedExecution = false;
        _paused = false;
        _pendingUnpause = false;
    }
}

[Flags]
public enum FrameAdvanceMode
{
    Update = 1,
    FixedUpdate = 2
}