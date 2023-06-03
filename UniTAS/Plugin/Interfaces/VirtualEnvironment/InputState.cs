using System;
using UniTAS.Plugin.Interfaces.Events;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;

namespace UniTAS.Plugin.Interfaces.VirtualEnvironment;

public abstract class InputState : IOnVirtualEnvStatusChange, IOnGameRestart
{
    protected abstract void ResetState();

    public void OnVirtualEnvStatusChange(bool runVirtualEnv)
    {
        if (!runVirtualEnv) return;

        ResetState();
    }

    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        ResetState();
    }
}