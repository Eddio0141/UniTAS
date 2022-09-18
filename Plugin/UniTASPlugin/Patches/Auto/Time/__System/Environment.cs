using HarmonyLib;
using System;

namespace UniTASPlugin.Patches.Auto.Time.__System;

[HarmonyPatch(typeof(Environment), nameof(Environment.TickCount), MethodType.Getter)]
class TickCountGetter
{
    static bool Prefix(ref int __result)
    {
        __result = (int)TimeSpan.FromTicks(Core.TAS.Main.Time.Ticks).TotalMilliseconds;
        return false;
    }
}
