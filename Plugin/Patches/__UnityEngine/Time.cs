using HarmonyLib;
using System;
using System.Reflection;
using UniTASPlugin.FakeGameState;
using UniTASPlugin.VersionSafeWrapper;

namespace UniTASPlugin.Patches.__UnityEngine;

#pragma warning disable IDE1006

[HarmonyPatch(typeof(UnityEngine.Time), "captureFramerate", MethodType.Setter)]
class captureFramerateSetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix()
    {
        // if TAS is running and we aren't setting the frametime, reject
        return !TAS.Main.Running || TimeWrap.SettingFrametime;
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), "captureDeltaTime", MethodType.Setter)]
class captureDeltaTimeSetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix()
    {
        // if TAS is running and we aren't setting the frametime, reject
        return !TAS.Main.Running || TimeWrap.SettingFrametime;
    }
}

[HarmonyPatch(typeof(UnityEngine.Time), "fixedUnscaledTime", MethodType.Getter)]
class fixedUnscaledTimeGetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref float __result)
    {
        __result = (float)TimeSpan.FromTicks(GameTime.Time.Ticks).TotalSeconds;
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
        __result = TimeSpan.FromTicks(GameTime.Time.Ticks).TotalSeconds;
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
        __result = (int)GameTime.FrameCount;
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
        __result = (int)GameTime.FrameCount;
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
        __result = (float)TimeSpan.FromTicks(GameTime.Time.Ticks).TotalSeconds;
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
        __result = TimeSpan.FromTicks(GameTime.Time.Ticks).TotalSeconds;
        return false;
    }
}