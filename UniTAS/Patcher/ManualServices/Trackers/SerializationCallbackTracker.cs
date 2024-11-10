using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.ManualServices.Trackers;

public static class SerializationCallbackTracker
{
    private static readonly HashSet<object> AfterDeserializationInvoked = new(new HashUtils.ReferenceComparer());
    private static readonly object AfterDeserializationInvokedLock = new();

    private static bool _initializedDelegates;

    // TODO: faster invoker
    private static MethodInfo _afterDeserialization;

    /// <returns>True if original function should run</returns>
    public static bool OnAfterDeserializeInvoke(object instance)
    {
        var newInstance = false;
        lock (AfterDeserializationInvokedLock)
        {
            if (!AfterDeserializationInvoked.Contains(instance))
            {
                newInstance = true;
                AfterDeserializationInvoked.Add(instance);
            }
        }

        // now scriptable object handling
        if (newInstance && instance is ScriptableObject so)
        {
            SaveScriptableObjectStatesManual.Save(so);
        }

        return !AfterSerializationManuallyInvoked.Contains(instance);
    }

    private static readonly HashSet<object> AfterSerializationManuallyInvoked = new(new HashUtils.ReferenceComparer());

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
                AfterSerializationManuallyInvoked.Add(obj);
            }

            AfterSerializationManuallyInvoked.Clear();
        }
    }
}