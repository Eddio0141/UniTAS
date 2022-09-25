using HarmonyLib;
using System;
using System.Reflection;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine;

static class Helper
{
    public static Type ResourceRequestType()
    {
        return AccessTools.TypeByName("UnityEngine.ResourceRequest");
    }
}

[HarmonyPatch(typeof(Resources), "LoadAsyncInternal")]
class LoadAsyncInternal
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(string path, Type type, ref object __result)
    {
        // returns ResourceRequest
        // should be fine with my instance and no tinkering
        __result = AccessTools.CreateInstance(Helper.ResourceRequestType());
        var resultTraverse = Traverse.Create(__result);
        resultTraverse.Field("m_Path").SetValue(path);
        resultTraverse.Field("m_Type").SetValue(type);
        var wrap = new AsyncOperationWrap((AsyncOperation)__result);
        wrap.AssignUID();
        return false;
    }
}

// TODO UnloadUnusedAssets needs to be implemented
