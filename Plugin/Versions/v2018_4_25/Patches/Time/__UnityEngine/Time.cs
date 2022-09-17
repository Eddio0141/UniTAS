using HarmonyLib;

namespace v2018_4_25.Patches.Time.__UnityEngine;

#pragma warning disable IDE1006

[HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.fixedUnscaledTime), MethodType.Getter)]
class fixedUnscaledTimeGetter
{
    static bool Prefix(ref float __result)
    {
        if (Core.TAS.Main.Running)
        {
            // get total seconds since start
            __result = (float)System.TimeSpan.FromTicks(Core.TAS.Main.Time.Ticks).TotalSeconds;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.frameCount), MethodType.Getter)]
class frameCountGetter
{
    static bool Prefix(ref int __result)
    {
        if (Core.TAS.Main.Running)
        {
            __result = (int)Core.TAS.Main.FrameCount;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.renderedFrameCount), MethodType.Getter)]
class renderedFrameCountGetter
{
    static bool Prefix(ref int __result)
    {
        if (Core.TAS.Main.Running)
        {
            __result = (int)Core.TAS.Main.FrameCount;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.realtimeSinceStartup), MethodType.Getter)]
class realtimeSinceStartupGetter
{
    static bool Prefix(ref float __result)
    {
        if (Core.TAS.Main.Running)
        {
            __result = (float)System.TimeSpan.FromTicks(Core.TAS.Main.Time.Ticks).TotalSeconds;

            return false;
        }

        return true;
    }
}