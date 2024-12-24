using System;
using System.Collections;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services.GameExecutionControllers;

namespace UniTAS.Patcher.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton(timing: RegisterTiming.Entry)]
public class MonoBehaviourController : IMonoBehaviourController, IOnGameRestartResume
{
    public bool PausedExecution { get; set; }
    public bool PausedUpdate { get; set; }

    public HashSet<IEnumerator> IgnoreCoroutines { get; } = new();

    public void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        // make sure we reset the paused update state on restart
        if (preMonoBehaviourResume) return;
        PausedUpdate = false;
    }
}