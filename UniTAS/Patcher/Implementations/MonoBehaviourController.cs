using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Services;

namespace UniTAS.Patcher.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class MonoBehaviourController : IMonoBehaviourController, IOnGameRestartResume
{
    public bool PausedExecution
    {
        get => Utils.MonoBehaviourController.PausedExecution;
        set => Utils.MonoBehaviourController.PausedExecution = value;
    }

    public bool PausedUpdate
    {
        get => Utils.MonoBehaviourController.PausedUpdate;
        set => Utils.MonoBehaviourController.PausedUpdate = value;
    }

    public void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        // make sure we reset the paused update state on restart
        if (preMonoBehaviourResume) return;
        PausedUpdate = false;
    }
}