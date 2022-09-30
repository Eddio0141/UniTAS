using HarmonyLib;
using System;
using System.Reflection;
using UniTASPlugin.FakeGameState;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine;

#pragma warning disable IDE1006

[HarmonyPatch(typeof(Time), nameof(Time.captureFramerate), MethodType.Setter)]
class set_captureFramerate
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix()
    {
        // if TAS is running / preparing and we aren't setting the frametime, reject
        return !(TAS.Running || TAS.PreparingRun);
    }
}

[HarmonyPatch(typeof(Time), "captureDeltaTime", MethodType.Setter)]
class set_captureDeltaTime
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix()
    {
        // if TAS is running / preparing and we aren't setting the frametime, reject
        return !(TAS.Running || TAS.PreparingRun);
    }
}

[HarmonyPatch(typeof(Time), "fixedUnscaledTime", MethodType.Getter)]
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

[HarmonyPatch(typeof(Time), "unscaledTime", MethodType.Getter)]
class get_unscaledTime
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static readonly Traverse inFixedTimeStep = Traverse.Create(typeof(Time)).Property("inFixedTimeStep");
    static readonly Traverse fixedUnscaledTime = Traverse.Create(typeof(Time)).Property("fixedUnscaledTime");

    static void Postfix(ref float __result)
    {
        // When called from inside MonoBehaviour's FixedUpdate, it returns Time.fixedUnscaledTime
        __result = inFixedTimeStep.PropertyExists() && inFixedTimeStep.GetValue<bool>()
            ? fixedUnscaledTime.GetValue<float>()
            : (float)((double)__result - GameTime.UnscaledTimeOffset);
    }
}

[HarmonyPatch(typeof(Time), "unscaledTimeAsDouble", MethodType.Getter)]
class get_unscaledTimeAsDouble
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static readonly Traverse inFixedTimeStep = Traverse.Create(typeof(Time)).Property("inFixedTimeStep");
    static readonly Traverse fixedUnscaledTimeAsDouble = Traverse.Create(typeof(Time)).Property("fixedUnscaledTimeAsDouble");

    static void Postfix(ref double __result)
    {
        // When called from inside MonoBehaviour's FixedUpdate, it returns Time.fixedUnscaledTimeAsDouble
        if (inFixedTimeStep.PropertyExists() && inFixedTimeStep.GetValue<bool>())
            __result = fixedUnscaledTimeAsDouble.GetValue<double>();
        else
            __result -= GameTime.UnscaledTimeOffset;
    }
}

[HarmonyPatch(typeof(Time), "fixedUnscaledTimeAsDouble", MethodType.Getter)]
class get_fixedUnscaledTimeAsDouble
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Postfix(ref double __result)
    {
        __result -= GameTime.FixedUnscaledTimeOffset;
    }
}

[HarmonyPatch(typeof(Time), nameof(Time.frameCount), MethodType.Getter)]
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

[HarmonyPatch(typeof(Time), nameof(Time.renderedFrameCount), MethodType.Getter)]
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

[HarmonyPatch(typeof(Time), nameof(Time.realtimeSinceStartup), MethodType.Getter)]
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

[HarmonyPatch(typeof(Time), "realtimeSinceStartupAsDouble", MethodType.Getter)]
class get_realtimeSinceStartupAsDouble
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Postfix(ref double __result)
    {
        __result -= GameTime.SecondsSinceStartUpOffset;
    }
}

[HarmonyPatch(typeof(Time), nameof(Time.time), MethodType.Getter)]
class get_time
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Postfix(ref float __result)
    {
        __result = (float)((double)__result - GameTime.ScaledTimeOffset);
    }
}

[HarmonyPatch(typeof(Time), "timeAsDouble", MethodType.Getter)]
class get_timeAsDouble
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Postfix(ref double __result)
    {
        __result -= GameTime.ScaledTimeOffset;
    }
}

[HarmonyPatch(typeof(Time), nameof(Time.fixedTime), MethodType.Getter)]
class get_fixedTime
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Postfix(ref float __result)
    {
        __result = (float)((double)__result - GameTime.ScaledFixedTimeOffset);
    }
}

[HarmonyPatch(typeof(Time), "fixedTimeAsDouble", MethodType.Getter)]
class get_fixedTimeAsDouble
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Postfix(ref double __result)
    {
        __result -= GameTime.ScaledFixedTimeOffset;
    }
}