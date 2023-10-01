using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Interfaces.Events.UnityEvents;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Models.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.UnityFix;

[Singleton]
public partial class SaveScriptableObjectStates : IOnSceneLoad, INewScriptableObjectTracker, IOnAwakeUnconditional,
    IOnPreGameRestart
{
    private readonly List<StoredState> _storedStates = new();
    private readonly List<Object> _destroyObjectsOnRestart = new();

    private readonly ILogger _logger;

    public SaveScriptableObjectStates(ILogger logger)
    {
        _logger = logger;
    }

    private bool _initialized;

    public void AwakeUnconditional()
    {
        if (_initialized) return;
        _initialized = true;

        // initial save
        SaveAll();
    }

    private void SaveAll()
    {
        _logger.LogDebug("Saving all ScriptableObject states");

        var allScriptableObjects = ResourcesUtils.FindObjectsOfTypeAll<ScriptableObject>();
        _logger.LogDebug($"Found {allScriptableObjects.Length} ScriptableObjects");

        foreach (var obj in allScriptableObjects)
        {
            Save(obj);
        }
    }

    private void Save(ScriptableObject obj)
    {
        foreach (var x in _storedStates)
        {
            if (x.ScriptableObject == obj) return;
        }

        _storedStates.Add(new(obj, _logger));
    }

    public void NewScriptableObject(ScriptableObject scriptableObject)
    {
        _destroyObjectsOnRestart.Add(scriptableObject);
    }

    public void OnPreGameRestart()
    {
        _logger.LogDebug("Destroying all ScriptableObject that was created during runtime");
        foreach (var obj in _destroyObjectsOnRestart)
        {
            Object.Destroy(obj);
        }

        _destroyObjectsOnRestart.Clear();

        _logger.LogDebug("Loading all ScriptableObject states");

        foreach (var storedState in _storedStates)
        {
            storedState.Load();
        }
    }

    public void OnSceneLoad(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode)
    {
        if (loadSceneMode == LoadSceneMode.Single)
        {
            _storedStates.Clear();
        }

        SaveAll();
    }
}