using System;
using UniTAS.Plugin.Interfaces.Events;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;

namespace UniTAS.Plugin.Services.VirtualEnvironment.InnerState.Input;

public abstract class InputDevice : IOnVirtualEnvStatusChange, IOnPreUpdates, IOnGameRestart
{
    public abstract void Update();
    public abstract void ResetState();

    public void OnVirtualEnvStatusChange(bool runVirtualEnv)
    {
        if (!runVirtualEnv) return;

        ResetState();
    }

    public void PreUpdate()
    {
        Update();
    }

    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        ResetState();
    }
}