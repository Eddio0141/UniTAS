using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.ReverseInvoker;
using TimeOrig = UnityEngine.Time;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Local
// ReSharper disable CommentTypo

namespace UniTASPlugin.Patches.UnityEngine;

#pragma warning disable IDE1006

[HarmonyPatch]
internal static class Time
{
    [HarmonyPatch(typeof(TimeOrig), nameof(TimeOrig.captureFramerate), MethodType.Setter)]
    private class set_captureFramerate
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix()
        {
            var kernel = Plugin.Kernel;
            if (kernel.GetInstance<PatchReverseInvoker>().Invoking)
                return true;
            // if TAS is running / preparing and we aren't setting the frametime, reject
            // TODO below
            //return !(TAS.Running || TAS.PreparingRun);
            return !(kernel.GetInstance<VirtualEnvironment>().RunVirtualEnvironment &&
                     !kernel.GetInstance<PatchReverseInvoker>().Invoking);
        }
    }

    [HarmonyPatch(typeof(TimeOrig), "captureDeltaTime", MethodType.Setter)]
    private class set_captureDeltaTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix()
        {
            var kernel = Plugin.Kernel;
            if (kernel.GetInstance<PatchReverseInvoker>().Invoking)
                return true;
            // if TAS is running / preparing and we aren't setting the frametime, reject
            // TODO below
            //return !(TAS.Running || TAS.PreparingRun);
            return !(kernel.GetInstance<VirtualEnvironment>().RunVirtualEnvironment &&
                     !kernel.GetInstance<PatchReverseInvoker>().Invoking);
        }
    }

    [HarmonyPatch(typeof(TimeOrig), "fixedUnscaledTime", MethodType.Getter)]
    private class get_fixedUnscaledTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static void Postfix(ref float __result)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking)
                return;
            var gameTime = Plugin.Kernel.GetInstance<IVirtualEnvironmentFactory>().GetVirtualEnv().GameTime;
            __result = (float)(__result - gameTime.FixedUnscaledTimeOffset);
        }
    }

    [HarmonyPatch(typeof(TimeOrig), "unscaledTime", MethodType.Getter)]
    private class get_unscaledTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static readonly Traverse
            inFixedTimeStep = Traverse.Create(typeof(TimeOrig)).Property("inFixedTimeStep");

        private static readonly Traverse fixedUnscaledTime =
            Traverse.Create(typeof(TimeOrig)).Property("fixedUnscaledTime");

        private static void Postfix(ref float __result)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking)
                return;
            // When called from inside MonoBehaviour's FixedUpdate, it returns TimeOrig.fixedUnscaledTime
            var gameTime = Plugin.Kernel.GetInstance<IVirtualEnvironmentFactory>().GetVirtualEnv().GameTime;
            __result = inFixedTimeStep.PropertyExists() && inFixedTimeStep.GetValue<bool>()
                ? fixedUnscaledTime.GetValue<float>()
                : (float)(__result - gameTime.UnscaledTimeOffset);
        }
    }

    [HarmonyPatch(typeof(TimeOrig), "unscaledTimeAsDouble", MethodType.Getter)]
    private class get_unscaledTimeAsDouble
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static readonly Traverse
            inFixedTimeStep = Traverse.Create(typeof(TimeOrig)).Property("inFixedTimeStep");

        private static readonly Traverse fixedUnscaledTimeAsDouble =
            Traverse.Create(typeof(TimeOrig)).Property("fixedUnscaledTimeAsDouble");

        private static void Postfix(ref double __result)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking)
                return;
            // When called from inside MonoBehaviour's FixedUpdate, it returns TimeOrig.fixedUnscaledTimeAsDouble
            if (inFixedTimeStep.PropertyExists() && inFixedTimeStep.GetValue<bool>())
                __result = fixedUnscaledTimeAsDouble.GetValue<double>();
            else
            {
                var gameTime = Plugin.Kernel.GetInstance<IVirtualEnvironmentFactory>().GetVirtualEnv().GameTime;
                __result -= gameTime.UnscaledTimeOffset;
            }
        }
    }

    [HarmonyPatch(typeof(TimeOrig), "fixedUnscaledTimeAsDouble", MethodType.Getter)]
    private class get_fixedUnscaledTimeAsDouble
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static void Postfix(ref double __result)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking)
                return;
            var gameTime = Plugin.Kernel.GetInstance<IVirtualEnvironmentFactory>().GetVirtualEnv().GameTime;
            __result -= gameTime.FixedUnscaledTimeOffset;
        }
    }

    [HarmonyPatch(typeof(TimeOrig), nameof(TimeOrig.frameCount), MethodType.Getter)]
    private class get_frameCount
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static void Postfix(ref int __result)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking)
                return;
            var gameTime = Plugin.Kernel.GetInstance<IVirtualEnvironmentFactory>().GetVirtualEnv().GameTime;
            __result = (int)((ulong)__result - gameTime.FrameCountRestartOffset);
        }
    }

    [HarmonyPatch(typeof(TimeOrig), nameof(TimeOrig.renderedFrameCount), MethodType.Getter)]
    private class get_renderedFrameCount
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static void Postfix(ref int __result)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking)
                return;
            var gameTime = Plugin.Kernel.GetInstance<IVirtualEnvironmentFactory>().GetVirtualEnv().GameTime;
            __result = (int)((ulong)__result - gameTime.RenderedFrameCountOffset);
        }
    }

    [HarmonyPatch(typeof(TimeOrig), nameof(TimeOrig.realtimeSinceStartup), MethodType.Getter)]
    private class get_realtimeSinceStartup
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static void Postfix(ref float __result)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking)
                return;
            var gameTime = Plugin.Kernel.GetInstance<IVirtualEnvironmentFactory>().GetVirtualEnv().GameTime;
            __result = (float)(__result - gameTime.SecondsSinceStartUpOffset);
        }
    }

    [HarmonyPatch(typeof(TimeOrig), "realtimeSinceStartupAsDouble", MethodType.Getter)]
    private class get_realtimeSinceStartupAsDouble
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static void Postfix(ref double __result)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking)
                return;
            var gameTime = Plugin.Kernel.GetInstance<IVirtualEnvironmentFactory>().GetVirtualEnv().GameTime;
            __result -= gameTime.SecondsSinceStartUpOffset;
        }
    }

    [HarmonyPatch(typeof(TimeOrig), nameof(TimeOrig.time), MethodType.Getter)]
    private class get_time
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static void Postfix(ref float __result)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking)
                return;
            var gameTime = Plugin.Kernel.GetInstance<IVirtualEnvironmentFactory>().GetVirtualEnv().GameTime;
            __result = (float)(__result - gameTime.ScaledTimeOffset);
        }
    }

    [HarmonyPatch(typeof(TimeOrig), "timeAsDouble", MethodType.Getter)]
    private class get_timeAsDouble
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static void Postfix(ref double __result)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking)
                return;
            var gameTime = Plugin.Kernel.GetInstance<IVirtualEnvironmentFactory>().GetVirtualEnv().GameTime;
            __result -= gameTime.ScaledTimeOffset;
        }
    }

    [HarmonyPatch(typeof(TimeOrig), nameof(TimeOrig.fixedTime), MethodType.Getter)]
    private class get_fixedTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static void Postfix(ref float __result)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking)
                return;
            var gameTime = Plugin.Kernel.GetInstance<IVirtualEnvironmentFactory>().GetVirtualEnv().GameTime;
            __result = (float)(__result - gameTime.ScaledFixedTimeOffset);
        }
    }

    [HarmonyPatch(typeof(TimeOrig), "fixedTimeAsDouble", MethodType.Getter)]
    private class get_fixedTimeAsDouble
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static void Postfix(ref double __result)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking)
                return;
            var gameTime = Plugin.Kernel.GetInstance<IVirtualEnvironmentFactory>().GetVirtualEnv().GameTime;
            __result -= gameTime.ScaledFixedTimeOffset;
        }
    }
}