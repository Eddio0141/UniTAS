using HarmonyLib;
using System;

namespace UniTASPlugin.Patches.Auto.Time.__System;

[HarmonyPatch(typeof(DateTime), nameof(DateTime.Now), MethodType.Getter)]
class NowGetter
{
    static bool Prefix(ref DateTime __result)
    {
        __result = UniTASPlugin.TAS.Main.Time;
        return false;
    }
}
