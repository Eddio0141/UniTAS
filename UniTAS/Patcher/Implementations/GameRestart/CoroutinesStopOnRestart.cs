using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GameRestart;

[Singleton]
public class CoroutinesStopOnRestart : ICoroutineRunningObjectsTracker, IOnPreGameRestart
{
    private readonly HashSet<MonoBehaviour> _instances = [];

    public void NewCoroutine(MonoBehaviour instance)
    {
        if (instance == null) return;
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