using HarmonyLib;
using System;
using System.Reflection;
using Ninject;
using UniTASPlugin.Movie;
using UniTASPlugin.Movie.ScriptEngine;
using UniTASPlugin.GameEnvironment;

namespace UniTASPlugin.Patches.__UnityEngine.__EventSystems;

static class Helper
{
    public static Type GetEventSystem()
    {
        return AccessTools.TypeByName("UnityEngine.EventSystems.EventSystem");
    }
}

[HarmonyPatch]
class isFocusedGetter
{
    static MethodBase TargetMethod()
    {
        return AccessTools.PropertyGetter(Helper.GetEventSystem(), "isFocused");
    }

    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref bool __result)
    {
        if (!Plugin.Instance.Kernel.Get<VirtualEnvironment>().RunVirtualEnvironment) return true;
        __result = true;
        return false;
    }
}