using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Services.Trackers;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GameRestart;

[Singleton]
public class CoroutinesStopOnRestart : ICoroutineRunningObjectsTracker, IOnPreGameRestart
{
    private readonly List<MonoBehaviour> _instances = new();

    public void NewCoroutine(MonoBehaviour instance)
    {
        if (!_instances.Contains(instance) && instance != null)
        {
            _instances.Add(instance);
        }
    }

    public void OnPreGameRestart()
    {
        foreach (var coroutine in _instances)
        {
            if (coroutine == null) continue;
            coroutine.StopAllCoroutines();
        }
    }
}