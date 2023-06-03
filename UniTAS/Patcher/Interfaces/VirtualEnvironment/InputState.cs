using System;
using UniTAS.Patcher.Interfaces.Events;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;

namespace UniTAS.Patcher.Interfaces.VirtualEnvironment;

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