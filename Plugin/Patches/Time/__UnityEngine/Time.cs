using HarmonyLib;
using System;
using System.Reflection;

namespace UniTASPlugin.Patches.Time.__UnityEngine;

#pragma warning disable IDE1006

[HarmonyPatch(typeof(UnityEngine.Time))]
class TimePatch
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Auxilary.Cleanup_IgnoreNotFound(original, ex);
    }

    /*
    [HarmonyPrefix]
    [HarmonyPatch("fixedUnscaledTime", MethodType.Getter)]
    static bool Prefix_fixedUnscaledTimeGetter(ref float __result)
    {
        __result = (float)System.TimeSpan.FromTicks(TAS.Main.Time.Ticks).TotalSeconds;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("fixedUnscaledTimeAsDouble", MethodType.Getter)]
    static bool Prefix_fixedUnscaledTimeAsDoubleGetter(ref double __result)
    {
        __result = System.TimeSpan.FromTicks(TAS.Main.Time.Ticks).TotalSeconds;
        return false;
    }
    */

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UnityEngine.Time.frameCount), MethodType.Getter)]
    static bool Prefix_frameCountGetter(ref int __result)
    {
        __result = (int)TAS.Main.FrameCount;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UnityEngine.Time.renderedFrameCount), MethodType.Getter)]
    static bool Prefix_renderedFrameCountGetter(ref int __result)
    {
        __result = (int)TAS.Main.FrameCount;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UnityEngine.Time.realtimeSinceStartup), MethodType.Getter)]
    static bool Prefix_realtimeSinceStartupGetter(ref float __result)
    {
        __result = (float)System.TimeSpan.FromTicks(TAS.Main.Time.Ticks).TotalSeconds;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("realtimeSinceStartupAsDouble", MethodType.Getter)]
    static bool Prefix_realtimeSinceStartupAsDoubleGetter(ref double __result)
    {
        __result = System.TimeSpan.FromTicks(UniTASPlugin.TAS.Main.Time.Ticks).TotalSeconds;
        return false;
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), "fixedUnscaledTime", MethodType.Getter)]
class Test
{
    static bool Prefix(ref float __result)
    {
        __result = (float)TimeSpan.FromTicks(TAS.Main.Time.Ticks).TotalSeconds;
        return false;
    }
}