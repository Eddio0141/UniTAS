using HarmonyLib;
using System;
using System.Reflection;

namespace UniTASPlugin.Patches.Time.__UnityEngine;

#pragma warning disable IDE1006

[HarmonyPatch(typeof(UnityEngine.Time), "fixedUnscaledTime", MethodType.Getter)]
class fixedUnscaledTimeGetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref float __result)
    {
        __result = (float)TimeSpan.FromTicks(TAS.Main.Time.Ticks).TotalSeconds;
        return false;
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), "fixedUnscaledTimeAsDouble", MethodType.Getter)]
class fixedUnscaledTimeAsDoubleGetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref double __result)
    {
        __result = TimeSpan.FromTicks(TAS.Main.Time.Ticks).TotalSeconds;
        return false;
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.frameCount), MethodType.Getter)]
class frameCountGetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref int __result)
    {
        __result = (int)TAS.Main.FrameCount;
        return false;
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.renderedFrameCount), MethodType.Getter)]
class renderedFrameCountGetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref int __result)
    {
        __result = (int)TAS.Main.FrameCount;
        return false;
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.realtimeSinceStartup), MethodType.Getter)]
class realtimeSinceStartupGetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref float __result)
    {
        __result = (float)TimeSpan.FromTicks(TAS.Main.Time.Ticks).TotalSeconds;
        return false;
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), "realtimeSinceStartupAsDouble", MethodType.Getter)]
class realtimeSinceStartupAsDoubleGetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref double __result)
    {
        __result = TimeSpan.FromTicks(TAS.Main.Time.Ticks).TotalSeconds;
        return false;
    }
}