using HarmonyLib;
using System;
using System.Reflection;

namespace UniTASPlugin.Patches.__System;

[HarmonyPatch(typeof(Environment))]
class EnvironmentPatch
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_NeedsToBePatched(original, ex);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Environment.TickCount), MethodType.Getter)]
    static bool Prefix_TickCountGetter(ref int __result)
    {
        __result = (int)TimeSpan.FromTicks(TAS.Main.Time.Ticks).TotalMilliseconds;
        return false;
    }
}
