using System.Collections.Generic;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.ManualServices.Trackers;

public static class SerializationCallbackTrackerManual
{
    private static readonly HashSet<object>
        AfterDeserializationInvoked = new(new HashUtils.ReferenceComparer<object>());

    // ReSharper disable once InconsistentNaming
    public static void OnAfterDeserializeInvoke(object __instance)
    {
        var newInstance = false;
        lock (AfterDeserializationInvoked)
        {
            if (!AfterDeserializationInvoked.Contains(__instance))
            {
                newInstance = true;
                AfterDeserializationInvoked.Add(__instance);
            }
        }

        // now scriptable object handling
        if (newInstance && __instance is ScriptableObject so)
        {
            SaveScriptableObjectStatesManual.Save(so);
        }
    }
}