using HarmonyLib;
using System;
using System.Reflection;
using UniTASPlugin.FakeGameState;
using UniTASPlugin.VersionSafeWrapper;

namespace UniTASPlugin.Patches.__UnityEngine;

#pragma warning disable IDE1006

[HarmonyPatch(typeof(UnityEngine.Time), "captureFramerate", MethodType.Setter)]
class set_captureFramerate
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix()
    {
        // if TAS is running / preparing and we aren't setting the frametime, reject
        return !((TAS.Main.Running || TAS.Main.PreparingRun) && !TimeWrap.SettingFrametime);
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), "captureDeltaTime", MethodType.Setter)]
class set_captureDeltaTime
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix()
    {
        // if TAS is running / preparing and we aren't setting the frametime, reject
        return !((TAS.Main.Running || TAS.Main.PreparingRun) && !TimeWrap.SettingFrametime);
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), "fixedUnscaledTime", MethodType.Getter)]
class get_fixedUnscaledTime
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Postfix(ref float __result)
    {
        __result = (float)((double)__result - GameTime.FixedUnscaledTimeOffset);
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), "fixedUnscaledTimeAsDouble", MethodType.Getter)]
class get_fixedUnscaledTimeAsDouble
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Postfix(ref double __result)
    {
        __result = __result - GameTime.FixedUnscaledTimeOffset;
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.frameCount), MethodType.Getter)]
class get_frameCount
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Postfix(ref int __result)
    {
        __result = (int)((ulong)__result - GameTime.FrameCountRestartOffset);
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.renderedFrameCount), MethodType.Getter)]
class get_renderedFrameCount
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Postfix(ref int __result)
    {
        __result = (int)((ulong)__result - GameTime.RenderedFrameCountOffset);
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.realtimeSinceStartup), MethodType.Getter)]
class get_realtimeSinceStartup
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Postfix(ref float __result)
    {
        __result = (float)((double)__result - GameTime.SecondsSinceStartUpOffset);
    }
}

/*
[HarmonyPatch(typeof(UnityEngine.Time), "realtimeSinceStartupAsDouble", MethodType.Getter)]
class get_realtimeSinceStartupAsDouble
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref double __result)
    {
        __result = GameTime.SecondsSinceStartUpTimeScale;
        return false;
    }
}

/*
class Dummy
{
    public static extern float time { get; }
    public static extern float timeSinceLevelLoad { get; }
    public static extern float fixedTime { get; }
    public static extern float unscaledTime { get; }
}
*/

/*
class Latest
{
    public static extern double timeAsDouble { get; }
    public static extern double timeSinceLevelLoadAsDouble { get; }
    public static extern double fixedTimeAsDouble { get; }
    public static extern double unscaledTimeAsDouble { get; }
    public static extern float fixedUnscaledTime { get; }
    public static extern double fixedUnscaledTimeAsDouble { get; }
    public static extern float unscaledDeltaTime { get; }
}
*/