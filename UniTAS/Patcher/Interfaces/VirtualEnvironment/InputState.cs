using System;
using UniTAS.Patcher.Interfaces.Events;
using UniTAS.Patcher.Interfaces.Events.Movie;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;

namespace UniTAS.Patcher.Interfaces.VirtualEnvironment;

public abstract class InputState : IOnVirtualEnvStatusChange, IOnGameRestart, IOnMovieUpdate
{
    protected abstract void ResetState();

    /// <summary>
    /// Function invoked when a frame advances.
    /// </summary>
    protected virtual void Update() { }

    public void OnVirtualEnvStatusChange(bool runVirtualEnv)
    {
        if (!runVirtualEnv) return;

        ResetState();
    }

    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        ResetState();
    }

    public virtual void MovieUpdate(bool fixedUpdate)
    {
        if (!fixedUpdate)
        {
            Update();
        }
    }
}
