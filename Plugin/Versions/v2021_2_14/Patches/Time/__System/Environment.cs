using HarmonyLib;
using System;

namespace v2021_2_14.Patches.Time.__System;

[HarmonyPatch(typeof(Environment), nameof(Environment.TickCount), MethodType.Getter)]
class TickCountGetter
{
    static bool Prefix(ref int __result)
    {
        if (Core.TAS.Main.Running)
        {
            __result = (int)TimeSpan.FromTicks(Core.TAS.Main.Time.Ticks).TotalMilliseconds;
            return false;
        }
        return true;
    }
}
