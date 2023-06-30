using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Trackers;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.Trackers;

[Singleton]
[ExcludeRegisterIfTesting]
public class RunInBackgroundHandler : IRunInBackgroundTracker, IOnGameRestart
{
    public bool RunInBackground { get; set; }

    public RunInBackgroundHandler(IPatchReverseInvoker patchReverseInvoker)
    {
        // have to be false or for some reason the TAS doesn't pause while the game doesn't have focus
        patchReverseInvoker.Invoke(value => Application.runInBackground = value, false);
    }

    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        if (preSceneLoad) return;
        RunInBackground = false;
    }
}