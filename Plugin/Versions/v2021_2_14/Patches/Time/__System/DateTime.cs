using HarmonyLib;
using System;

namespace v2021_2_14.Patches.Time.__System;

[HarmonyPatch(typeof(DateTime), nameof(DateTime.Now), MethodType.Getter)]
class NowGetter
{
    static bool Prefix(ref DateTime __result)
    {
        if (Core.TAS.Main.Running)
        {
            var totalSeconds = Core.TAS.Main.Time;
            var totalMilliseconds = totalSeconds * 1000;
            var totalTicks = (long)(totalMilliseconds * 10000);
            __result = new DateTime(totalTicks);

            return false;
        }

        return true;
    }
}
