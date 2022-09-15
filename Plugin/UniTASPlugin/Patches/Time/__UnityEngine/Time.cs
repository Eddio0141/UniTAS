using HarmonyLib;

namespace UniTASPlugin.Patches.Time.__UnityEngine;

#pragma warning disable IDE1006

[HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.fixedUnscaledTime), MethodType.Getter)]
class fixedUnscaledTimeGetter
{
    static bool Prefix(ref float __result)
    {
        if (TAS.Main.Running)
        {
            __result = (float)TAS.Main.Time;

            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.fixedUnscaledTimeAsDouble), MethodType.Getter)]
class fixedUnscaledTimeAsDoubleGetter
{
    static bool Prefix(ref double __result)
    {
        if (TAS.Main.Running)
        {
            __result = TAS.Main.Time;

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
        if (TAS.Main.Running)
        {
            __result = (int)TAS.Main.FrameCount;

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
        if (TAS.Main.Running)
        {
            __result = (int)TAS.Main.FrameCount;

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
        if (TAS.Main.Running)
        {
            __result = (float)TAS.Main.Time;

            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.realtimeSinceStartupAsDouble), MethodType.Getter)]
class realtimeSinceStartupAsDoubleGetter
{
    static bool Prefix(ref double __result)
    {
        if (TAS.Main.Running)
        {
            __result = TAS.Main.Time;

            return false;
        }

        return true;
    }
}