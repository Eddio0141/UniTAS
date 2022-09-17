using HarmonyLib;
using System;

namespace v2018_4_25.Patches.Time.__System;

[HarmonyPatch(typeof(DateTime), nameof(DateTime.Now), MethodType.Getter)]
class NowGetter
{
    static bool Prefix(ref DateTime __result)
    {
        if (Core.TAS.Main.Running)
        {
            __result = Core.TAS.Main.Time;
            return false;
        }
        return true;
    }
}
