using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine.EventSystems;

namespace UniTASPlugin.Patches.TASInput.__UnityEngine.__EventSystems;

[HarmonyPatch(typeof(EventSystem))]
class EventSystemPatch
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Auxilary.Cleanup_IgnoreNotFound(original, ex);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EventSystem.isFocused), MethodType.Getter)]
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