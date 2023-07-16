using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Customization;
using UniTAS.Patcher.Services.FrameAdvancing;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.FrameAdvancing;

[Singleton(RegisterPriority.FrameAdvancing)]
public class FrameAdvancing : IFrameAdvancing, IOnUpdateUnconditional
{
    private bool _active;

    private uint _pendingPauseFrames;

    private bool _paused;
    private double _updateRestoreOffset;
    private bool _pendingUnpause;
    private readonly Action _unpauseActual;

    private readonly IMonoBehaviourController _monoBehaviourController;
    private readonly ISyncFixedUpdateCycle _syncFixedUpdate;

    public FrameAdvancing(IMonoBehaviourController monoBehaviourController, IBinds binds,
        ISyncFixedUpdateCycle syncFixedUpdate)
    {
        // TODO clean these binds up
        binds.Create(new("FrameAdvance", KeyCode.Slash));
        binds.Create(new("FrameAdvanceResume", KeyCode.Period));

        _monoBehaviourController = monoBehaviourController;
        _syncFixedUpdate = syncFixedUpdate;
        _unpauseActual = UnpauseActual;
    }

    public void FrameAdvance(uint frames)
    {
        if (!_active)
        {
            // ok frame advancing needs to be activated
            _active = true;
            frames = 0;
        }

        _pendingPauseFrames = frames;
    }

    public void Resume()
    {
        _active = false;
    }

    // unpause depends on syncing update, and this class needs to run AFTER the sync update
    public void UpdateUnconditional()
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

        Pause();
    }

    // TODO pause timing choose via frame advance method
    private void Pause()
    {
        if (_paused) return;
        _paused = true;

        // the pause happens before the updates

        _updateRestoreOffset = UpdateInvokeOffset.Offset;
        _monoBehaviourController.PausedExecution = true;
    }

    private void Unpause()
    {
        if (!_paused || _pendingUnpause) return;
        _pendingUnpause = true;

        _syncFixedUpdate.OnSync(_unpauseActual, _updateRestoreOffset);
    }

    private void UnpauseActual()
    {
        _monoBehaviourController.PausedExecution = false;
        _paused = false;
        _pendingUnpause = false;
    }
}