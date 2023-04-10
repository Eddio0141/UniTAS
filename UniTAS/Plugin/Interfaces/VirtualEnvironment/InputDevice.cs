using System;
using UniTAS.Plugin.Interfaces.Events;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.DontRunIfPaused;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;

namespace UniTAS.Plugin.Interfaces.VirtualEnvironment;

public abstract class InputDevice : IOnVirtualEnvStatusChange, IOnPreUpdatesActual, IOnGameRestart
{
    protected abstract void Update();
    protected abstract void ResetState();

    public void OnVirtualEnvStatusChange(bool runVirtualEnv)
    {
        if (!runVirtualEnv) return;

        ResetState();
    }

    public void PreUpdateActual()
    {
        Update();
    }

    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        ResetState();
    }
}