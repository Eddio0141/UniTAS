using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Customization;
using UniTAS.Patcher.Services.FrameAdvancing;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.FrameAdvancing;

[Singleton(RegisterPriority.FrameAdvancing)]
public class FrameAdvancing : IFrameAdvancing, IOnUpdateUnconditional
{
    private bool _active;
    private bool _wasActive;

    private uint _pendingPauseFrames;

    private readonly IMonoBehaviourController _monoBehaviourController;

    public FrameAdvancing(IMonoBehaviourController monoBehaviourController, IBinds binds)
    {
        binds.Create(new("FrameAdvance", KeyCode.Slash));
        _monoBehaviourController = monoBehaviourController;
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

    public void UpdateUnconditional()
    {
        if (!_active)
        {
            if (!_wasActive) return;
            _wasActive = false;
            // unpause if haven't done yet
            _monoBehaviourController.PausedExecution = false;

            return;
        }

        // wait for frames until pause
        if (_pendingPauseFrames > 0)
        {
            _pendingPauseFrames--;
            return;
        }

        _monoBehaviourController.PausedExecution = true;
    }
}