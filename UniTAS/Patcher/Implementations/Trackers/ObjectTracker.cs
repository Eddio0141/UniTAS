using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Implementations.Trackers;

[Singleton]
public class ObjectTracker(ILogger logger) : IObjectTrackerUpdate, IOnGameRestart
{
    private readonly List<Object> _dontDestroyGameObjects = [];

    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        if (!preSceneLoad) return;

        logger.LogDebug($"Destroying {_dontDestroyGameObjects.Count} DontDestroyOnLoad objects");

        foreach (var obj in _dontDestroyGameObjects)
        {
            if (obj == null) continue;
            logger.LogDebug($"Destroying {obj.name}");
            Object.Destroy(obj);
        }

        _dontDestroyGameObjects.Clear();
    }

    public void DontDestroyOnLoadAddRoot(Object obj)
    {
        _dontDestroyGameObjects.Add(obj);
    }
}