using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.ManualServices.Trackers;

public static class SerializationCallbackTracker
{
    private static readonly HashSet<object> AfterDeserializationInvoked = [];
    private static readonly object AfterDeserializationInvokedLock = new();

    private static bool _initializedDelegates;

    // TODO: faster invoker
    private static MethodInfo _afterDeserialization;

    public static void OnAfterDeserializeInvoke(object instance)
    {
        lock (AfterDeserializationInvokedLock)
        {
            if (AfterDeserializationInvoked.Contains(instance)) return;
            AfterDeserializationInvoked.Add(instance);
        }

        // now scriptable object handling
        if (instance is not ScriptableObject so) return;
        SaveScriptableObjectStatesManual.Save(so);
    }

    public static void InvokeAllAfterDeserialization()
    {
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

        lock (AfterDeserializationInvokedLock)
        {
            AfterDeserializationInvoked.RemoveWhere(o => o == null);

            StaticLogger.LogDebug($"Invoking {AfterDeserializationInvoked.Count} objects with after deserialization");

            foreach (var obj in AfterDeserializationInvoked)
            {
                _afterDeserialization.Invoke(obj, null);
            }
        }
    }
}