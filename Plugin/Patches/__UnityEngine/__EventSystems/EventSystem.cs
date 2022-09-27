using HarmonyLib;
using System;
using System.Reflection;

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
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref bool __result)
    {
        if (TAS.Running)
        {
            __result = true;
            return false;
        }
        return true;
    }
}