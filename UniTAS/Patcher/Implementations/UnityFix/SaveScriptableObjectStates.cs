using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.ManualServices;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.UnityFix;

[Singleton]
public class SaveScriptableObjectStates : INewScriptableObjectTracker, IOnAwakeUnconditional, IOnPreGameRestart
{
    private readonly List<Object> _destroyObjectsOnRestart = [];

    private readonly ILogger _logger;

    private readonly Assembly[] _ignoreAssemblies;

    private readonly IUpdateScriptableObjDestroyState _updateScriptableObjDestroyState;
    private readonly IPatchReverseInvoker _reverseInvoker;

    public SaveScriptableObjectStates(ILogger logger, IUpdateScriptableObjDestroyState updateScriptableObjDestroyState,
        IPatchReverseInvoker reverseInvoker)
    {
        _logger = logger;
        _updateScriptableObjDestroyState = updateScriptableObjDestroyState;
        _reverseInvoker = reverseInvoker;

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

        var allScriptableObjects = ResourcesUtils.FindObjectsOfTypeAll<ScriptableObject>()
            .Where(x => _ignoreAssemblies.All(a => !Equals(a, x.GetType().Assembly))).ToArray();
        _logger.LogDebug($"Found {allScriptableObjects.Length} ScriptableObjects");

        foreach (var obj in allScriptableObjects)
        {
            SaveScriptableObjectStatesManual.Save(obj);
        }
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
            _reverseInvoker.Invoke(Object.DestroyImmediate, obj);
        }

        _destroyObjectsOnRestart.Clear();

        _updateScriptableObjDestroyState.ClearState();
        SaveScriptableObjectStatesManual.LoadAll();
    }
}