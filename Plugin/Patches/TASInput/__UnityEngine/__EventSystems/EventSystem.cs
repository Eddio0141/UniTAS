using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine.EventSystems;

namespace UniTASPlugin.Patches.TASInput.__UnityEngine.__EventSystems;

[HarmonyPatch(typeof(EventSystem), nameof(EventSystem.isFocused), MethodType.Getter)]
class isFocusedGetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref bool __result)
    {
        if (TAS.Main.Running)
        {
            __result = true;
            return false;
        }
        return true;
    }
}