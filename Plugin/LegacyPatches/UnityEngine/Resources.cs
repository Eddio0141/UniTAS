using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.LegacySafeWrappers;
using UnityEngine;
using AsyncOpOrig = UnityEngine.AsyncOperation;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantAssignment

namespace UniTASPlugin.Patches.UnityEngine;

internal static class Helper
{
    public static Type ResourceRequestType()
    {
        return AccessTools.TypeByName("UnityEngine.ResourceRequest");
    }
}

[HarmonyPatch(typeof(Resources), "LoadAsyncInternal")]
internal class LoadAsyncInternalPatch
{
    private static Exception Cleanup(MethodBase original, Exception ex)
    {
        return PatcherHelper.CleanupIgnoreException(original, ex);
    }

    private static bool Prefix(string path, Type type, ref object __result)
    {
        // returns ResourceRequest
        // should be fine with my instance and no tinkering
        __result = AccessTools.CreateInstance(Helper.ResourceRequestType());
        var resultTraverse = Traverse.Create(__result);
        _ = resultTraverse.Field("m_Path").SetValue(path);
        _ = resultTraverse.Field("m_Type").SetValue(type);
        var wrap = new AsyncOperationWrap((AsyncOpOrig)__result);
        wrap.AssignUID();
        return false;
    }
}

// TODO UnloadUnusedAssets needs to be implemented