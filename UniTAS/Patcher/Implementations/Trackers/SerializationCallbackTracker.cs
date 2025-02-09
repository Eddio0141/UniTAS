using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.ManualServices;
using UniTAS.Patcher.Services.Trackers;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.Trackers;

[Singleton]
public class SerializationCallbackTracker : IOnGameRestart, ISerializationCallbackTracker
{
    private readonly HashSet<object>
        _afterDeserializationInvoked = new(new HashUtils.ReferenceComparer<object>());

    private readonly object _afterDeserializationInvokedLock = new();

    private bool _initializedDelegates;

    // TODO: faster invoker
    private MethodInfo _afterDeserialization;

    public bool OnAfterDeserializeInvoke(object instance)
    {
        var newInstance = false;
        lock (_afterDeserializationInvokedLock)
        {
            if (!_afterDeserializationInvoked.Contains(instance))
            {
                newInstance = true;
                _afterDeserializationInvoked.Add(instance);
            }
        }

        // now scriptable object handling
        if (newInstance && instance is ScriptableObject so)
        {
            SaveScriptableObjectStatesManual.Save(so);
        }

        return !_afterSerializationManuallyInvoked.Contains(instance);
    }

    private readonly HashSet<object>
        _afterSerializationManuallyInvoked = new(new HashUtils.ReferenceComparer<object>());

    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        if (!preSceneLoad) return;

        var bench = Bench.Measure();

        if (!_initializedDelegates)
        {
            _initializedDelegates = true;

            var iSerializationCallbackReceiver = AccessTools.TypeByName("UnityEngine.ISerializationCallbackReceiver");
            if (iSerializationCallbackReceiver != null)
            {
                _afterDeserialization = AccessTools.Method(iSerializationCallbackReceiver, "OnAfterDeserialize");
            }
        }

        if (_afterDeserialization == null)
        {
            StaticLogger.LogDebug("Skipping invoking all after deserialization methods, method was not found");
            return;
        }

        lock (_afterDeserializationInvokedLock)
        {
            _afterDeserializationInvoked.RemoveWhere(o => o == null);

            StaticLogger.LogDebug($"Invoking {_afterDeserializationInvoked.Count} objects with after deserialization");

            foreach (var obj in _afterDeserializationInvoked)
            {
                ExceptionUtils.UnityLogErrorOnThrow((m, o) => m.Invoke(o, null), _afterDeserialization, obj);
                _afterSerializationManuallyInvoked.Add(obj);
            }

            _afterSerializationManuallyInvoked.Clear();
        }

        bench.Dispose();
    }
}