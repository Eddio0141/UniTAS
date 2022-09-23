using HarmonyLib;
using System;
using System.Reflection;
using UniTASPlugin.FakeGameState;

namespace UniTASPlugin.Patches.__System;

[HarmonyPatch(typeof(DateTime))]
class DateTimePatch
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DateTime.Now), MethodType.Getter)]
    static bool Prefix_NowGetter(ref DateTime __result)
    {
        __result = GameTime.Time;
        return false;
    }
}
