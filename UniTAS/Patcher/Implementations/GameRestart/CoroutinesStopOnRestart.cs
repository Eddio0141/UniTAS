using System.Collections.Generic;
using System.Diagnostics;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GameRestart;

[Singleton]
public class CoroutinesStopOnRestart(ILogger logger) : ICoroutineRunningObjectsTracker, IOnPreGameRestart
{
    private readonly HashSet<MonoBehaviour> _instances = [];

    public void NewCoroutine(MonoBehaviour instance)
    {
        if (instance == null) return;
        logger.LogDebug($"new coroutine for {instance.GetType().SaneFullName()}");
        StaticLogger.Trace($"call from {new StackTrace()}");
        _instances.Add(instance);
    }

    public void OnPreGameRestart()
    {
        foreach (var coroutine in _instances)
        {
            if (coroutine == null) continue;
            coroutine.StopAllCoroutines();
        }

        _instances.Clear();
    }
}