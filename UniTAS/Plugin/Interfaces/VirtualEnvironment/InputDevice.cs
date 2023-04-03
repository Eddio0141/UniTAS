using System;
using UniTAS.Plugin.Interfaces.Events;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;

namespace UniTAS.Plugin.Interfaces.VirtualEnvironment;

public abstract class InputDevice : IOnVirtualEnvStatusChange, IOnPreUpdatesUnconditional, IOnGameRestart
{
    public abstract void Update();
    public abstract void ResetState();

    public void OnVirtualEnvStatusChange(bool runVirtualEnv)
    {
        if (!runVirtualEnv) return;

        ResetState();
    }

    public void PreUpdateUnconditional()
    {
        Update();
    }

    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        ResetState();
    }
}