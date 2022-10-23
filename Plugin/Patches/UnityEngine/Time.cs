using System;
using System.Reflection;
using HarmonyLib;
using Ninject;
using UniTASPlugin.FakeGameState;
using UniTASPlugin.GameEnvironment;
using TimeOrig = UnityEngine.Time;

namespace UniTASPlugin.Patches.UnityEngine;

#pragma warning disable IDE1006

[HarmonyPatch]
internal static class Time
{
    [HarmonyPatch(typeof(TimeOrig), nameof(TimeOrig.captureFramerate), MethodType.Setter)]
    class set_captureFramerate
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix()
        {
            var kernel = Plugin.Instance.Kernel;
            // if TAS is running / preparing and we aren't setting the frametime, reject
            // TODO below
            //return !(TAS.Running || TAS.PreparingRun);
            return !(kernel.Get<VirtualEnvironment>().RunVirtualEnvironment && !kernel.Get<PatchReverseInvoker>().Invoking);
        }
    }

    [HarmonyPatch(typeof(TimeOrig), "captureDeltaTime", MethodType.Setter)]
    class set_captureDeltaTime
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix()
        {
            var kernel = Plugin.Instance.Kernel;
            // if TAS is running / preparing and we aren't setting the frametime, reject
            // TODO below
            //return !(TAS.Running || TAS.PreparingRun);
            return !(kernel.Get<VirtualEnvironment>().RunVirtualEnvironment && !kernel.Get<PatchReverseInvoker>().Invoking);
        }
    }

    [HarmonyPatch(typeof(TimeOrig), "fixedUnscaledTime", MethodType.Getter)]
    class get_fixedUnscaledTime
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static void Postfix(ref float __result)
        {
            __result = (float)((double)__result - GameTime.FixedUnscaledTimeOffset);
        }
    }

    [HarmonyPatch(typeof(TimeOrig), "unscaledTime", MethodType.Getter)]
    class get_unscaledTime
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static readonly Traverse inFixedTimeStep = Traverse.Create(typeof(TimeOrig)).Property("inFixedTimeStep");
        static readonly Traverse fixedUnscaledTime = Traverse.Create(typeof(TimeOrig)).Property("fixedUnscaledTime");

        static void Postfix(ref float __result)
        {
            // When called from inside MonoBehaviour's FixedUpdate, it returns TimeOrig.fixedUnscaledTime
            __result = inFixedTimeStep.PropertyExists() && inFixedTimeStep.GetValue<bool>()
                ? fixedUnscaledTime.GetValue<float>()
                : (float)((double)__result - GameTime.UnscaledTimeOffset);
        }
    }

    [HarmonyPatch(typeof(TimeOrig), "unscaledTimeAsDouble", MethodType.Getter)]
    class get_unscaledTimeAsDouble
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static readonly Traverse inFixedTimeStep = Traverse.Create(typeof(TimeOrig)).Property("inFixedTimeStep");
        static readonly Traverse fixedUnscaledTimeAsDouble = Traverse.Create(typeof(TimeOrig)).Property("fixedUnscaledTimeAsDouble");

        static void Postfix(ref double __result)
        {
            // When called from inside MonoBehaviour's FixedUpdate, it returns TimeOrig.fixedUnscaledTimeAsDouble
            if (inFixedTimeStep.PropertyExists() && inFixedTimeStep.GetValue<bool>())
                __result = fixedUnscaledTimeAsDouble.GetValue<double>();
            else
                __result -= GameTime.UnscaledTimeOffset;
        }
    }

    [HarmonyPatch(typeof(TimeOrig), "fixedUnscaledTimeAsDouble", MethodType.Getter)]
    class get_fixedUnscaledTimeAsDouble
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static void Postfix(ref double __result)
        {
            __result -= GameTime.FixedUnscaledTimeOffset;
        }
    }

    [HarmonyPatch(typeof(TimeOrig), nameof(TimeOrig.frameCount), MethodType.Getter)]
    class get_frameCount
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static void Postfix(ref int __result)
        {
            __result = (int)((ulong)__result - GameTime.FrameCountRestartOffset);
        }
    }

    [HarmonyPatch(typeof(TimeOrig), nameof(TimeOrig.renderedFrameCount), MethodType.Getter)]
    class get_renderedFrameCount
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static void Postfix(ref int __result)
        {
            __result = (int)((ulong)__result - GameTime.RenderedFrameCountOffset);
        }
    }

    [HarmonyPatch(typeof(TimeOrig), nameof(TimeOrig.realtimeSinceStartup), MethodType.Getter)]
    class get_realtimeSinceStartup
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static void Postfix(ref float __result)
        {
            __result = (float)((double)__result - GameTime.SecondsSinceStartUpOffset);
        }
    }

    [HarmonyPatch(typeof(TimeOrig), "realtimeSinceStartupAsDouble", MethodType.Getter)]
    class get_realtimeSinceStartupAsDouble
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static void Postfix(ref double __result)
        {
            __result -= GameTime.SecondsSinceStartUpOffset;
        }
    }

    [HarmonyPatch(typeof(TimeOrig), nameof(TimeOrig.time), MethodType.Getter)]
    class get_time
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static void Postfix(ref float __result)
        {
            __result = (float)((double)__result - GameTime.ScaledTimeOffset);
        }
    }

    [HarmonyPatch(typeof(TimeOrig), "timeAsDouble", MethodType.Getter)]
    class get_timeAsDouble
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static void Postfix(ref double __result)
        {
            __result -= GameTime.ScaledTimeOffset;
        }
    }

    [HarmonyPatch(typeof(TimeOrig), nameof(TimeOrig.fixedTime), MethodType.Getter)]
    class get_fixedTime
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static void Postfix(ref float __result)
        {
            __result = (float)((double)__result - GameTime.ScaledFixedTimeOffset);
        }
    }

    [HarmonyPatch(typeof(TimeOrig), "fixedTimeAsDouble", MethodType.Getter)]
    class get_fixedTimeAsDouble
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static void Postfix(ref double __result)
        {
            __result -= GameTime.ScaledFixedTimeOffset;
        }
    }
}