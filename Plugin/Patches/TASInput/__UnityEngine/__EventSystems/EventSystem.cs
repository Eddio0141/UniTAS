using HarmonyLib;
using System;
using System.Reflection;
//using UnityEngine.EventSystems;

namespace UniTASPlugin.Patches.TASInput.__UnityEngine.__EventSystems;

#pragma warning disable IDE1006

[HarmonyPatch("UnityEngine.EventSystems.EventSystem")]
class EventSystemPatch
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Auxilary.Cleanup_IgnoreNotFound(original, ex);
    }

    //[HarmonyPrefix]
    //[HarmonyPatch("OnApplicationFocus", new Type[] { typeof(bool) })]
    public static void Prefix_OnApplicationFocus(ref bool hasFocus)
    {
        if (TAS.Main.Running)
        {
            // we dont want to lose focus while running the TAS
            // TODO movie can let this lose focus
            hasFocus = true;
        }
    }

    //[HarmonyPrefix]
    //[HarmonyPatch("isFocused", MethodType.Getter)]
    static bool Prefix_isFocusedGetter(ref bool __result)
    {
        if (TAS.Main.Running)
        {
            __result = true;
            return false;
        }
        return true;
    }
}