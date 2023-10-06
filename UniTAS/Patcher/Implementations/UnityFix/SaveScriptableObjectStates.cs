using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.UnityFix;

[Singleton]
public partial class SaveScriptableObjectStates : INewScriptableObjectTracker, IOnAwakeUnconditional, IOnPreGameRestart
{
    private readonly List<StoredState> _storedStates = new();
    private readonly List<Object> _destroyObjectsOnRestart = new();

    private readonly ILogger _logger;
    private readonly ITryFreeMalloc _freeMalloc;

    private readonly Assembly[] _ignoreAssemblies;

    public SaveScriptableObjectStates(ILogger logger, ITryFreeMalloc freeMalloc)
    {
        _logger = logger;
        _freeMalloc = freeMalloc;

        _ignoreAssemblies = new[]
        {
            AccessTools.TypeByName("TMPro.TMP_FontAsset")
        }.Where(x => x != null).Select(x => x.Assembly).ToArray();
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

        var allObjs = ResourcesUtils.FindObjectsOfTypeAll<Object>();
        var allScriptableObjects = allObjs.Where(x =>
                x is ScriptableObject && _ignoreAssemblies.All(a => !Equals(a, x.GetType().Assembly)))
            .Cast<ScriptableObject>().ToArray();
        _logger.LogDebug($"Found {allScriptableObjects.Length} ScriptableObjects");

        foreach (var obj in allScriptableObjects)
        {
            Save(obj, allObjs);
        }
    }

    private void Save(ScriptableObject obj, Object[] allObjs)
    {
        foreach (var x in _storedStates)
        {
            if (x.ScriptableObject == obj) return;
        }

        _storedStates.Add(new(obj, allObjs, _logger, _freeMalloc));
    }

    public void NewScriptableObject(ScriptableObject scriptableObject)
    {
        _destroyObjectsOnRestart.Add(scriptableObject);
    }

    public void OnPreGameRestart()
    {
        _logger.LogDebug(
            $"Destroying all {_destroyObjectsOnRestart.Count} ScriptableObject that was created during runtime");
        foreach (var obj in _destroyObjectsOnRestart)
        {
            Object.DestroyImmediate(obj);
        }

        _destroyObjectsOnRestart.Clear();

        _logger.LogDebug("Loading all ScriptableObject states");

        foreach (var storedState in _storedStates)
        {
            storedState.Load();
        }
    }

    // don't think this is required
    // testing with 2 scenes and 2 scriptable objects showed that on game start, both scriptable objects are loaded
    // public void OnSceneLoad(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
    //     LocalPhysicsMode localPhysicsMode)
    // {
    //     SaveAll();
    // }
}