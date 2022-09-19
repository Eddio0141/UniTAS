using HarmonyLib;
using System;
using System.Reflection;

namespace UniTASPlugin.Patches.Time.__System;

[HarmonyPatch(typeof(DateTime))]
class DateTimePatch
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_NeedsToBePatched(original, ex);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DateTime.Now), MethodType.Getter)]
    static bool Prefix_NowGetter(ref DateTime __result)
    {
        __result = TAS.Main.Time;
        return false;
    }
}
