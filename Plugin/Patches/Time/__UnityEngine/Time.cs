using HarmonyLib;

namespace UniTASPlugin.Patches.Time.__UnityEngine;

#pragma warning disable IDE1006

// TODO manual patch or something
/*
[HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.fixedUnscaledTime), MethodType.Getter)]
class fixedUnscaledTimeGetter
{
    static bool Prefix(ref float __result)
    {
        __result = (float)System.TimeSpan.FromTicks(TAS.Main.Time.Ticks).TotalSeconds;
        return false;
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.fixedUnscaledTimeAsDouble), MethodType.Getter)]
class fixedUnscaledTimeAsDoubleGetter
{
    static bool Prefix(ref double __result)
    {
        __result = System.TimeSpan.FromTicks(TAS.Main.Time.Ticks).TotalSeconds;
        return false;
    }
}
*/

[HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.frameCount), MethodType.Getter)]
class frameCountGetter
{
    static bool Prefix(ref int __result)
    {
        __result = (int)UniTASPlugin.TAS.Main.FrameCount;
        return false;
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.renderedFrameCount), MethodType.Getter)]
class renderedFrameCountGetter
{
    static bool Prefix(ref int __result)
    {
        __result = (int)UniTASPlugin.TAS.Main.FrameCount;
        return false;
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.realtimeSinceStartup), MethodType.Getter)]
class realtimeSinceStartupGetter
{
    static bool Prefix(ref float __result)
    {
        __result = (float)System.TimeSpan.FromTicks(UniTASPlugin.TAS.Main.Time.Ticks).TotalSeconds;
        return false;
    }
}

// TODO also this too
/*
[HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.realtimeSinceStartupAsDouble), MethodType.Getter)]
class realtimeSinceStartupAsDoubleGetter
{
    static bool Prefix(ref double __result)
    {
        __result = System.TimeSpan.FromTicks(UniTASPlugin.TAS.Main.Time.Ticks).TotalSeconds;
        return false;
    }
}
*/