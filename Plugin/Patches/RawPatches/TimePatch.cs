using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.LegacyPatches;
using UniTASPlugin.Patches.PatchTypes;
using UniTASPlugin.ReverseInvoker;
using UnityEngine;

namespace UniTASPlugin.Patches.RawPatches;

[RawPatch]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class TimePatch
{
    private static readonly IReverseInvokerFactory
        ReverseInvokerFactory = Plugin.Kernel.GetInstance<IReverseInvokerFactory>();

    private static readonly IVirtualEnvironmentFactory VirtualEnvironmentFactory =
        Plugin.Kernel.GetInstance<IVirtualEnvironmentFactory>();

    [HarmonyPatch(typeof(Time), nameof(Time.captureFramerate), MethodType.Setter)]
    private class set_captureFramerate
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix()
        {
            var rev = ReverseInvokerFactory.GetReverseInvoker();
            if (rev.Invoking)
                return true;
            // if TAS is running / preparing and we aren't setting the frametime, reject
            return !(VirtualEnvironmentFactory.GetVirtualEnv().RunVirtualEnvironment && !rev.Invoking);
        }
    }

    [HarmonyPatch(typeof(Time), "captureDeltaTime", MethodType.Setter)]
    private class set_captureDeltaTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix()
        {
            var rev = ReverseInvokerFactory.GetReverseInvoker();
            if (rev.Invoking)
                return true;
            return !(VirtualEnvironmentFactory.GetVirtualEnv().RunVirtualEnvironment && !rev.Invoking);
        }
    }

    [HarmonyPatch(typeof(Time), "fixedUnscaledTime", MethodType.Getter)]
    private class get_fixedUnscaledTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static void Postfix(ref float __result)
        {
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return;
            var gameTime = VirtualEnvironmentFactory.GetVirtualEnv().GameTime;
            __result = (float)(__result - gameTime.FixedUnscaledTimeOffset);
        }
    }

    [HarmonyPatch(typeof(Time), "unscaledTime", MethodType.Getter)]
    private class get_unscaledTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static readonly Traverse
            inFixedTimeStep = Traverse.Create(typeof(Time)).Property("inFixedTimeStep");

        private static readonly Traverse fixedUnscaledTime =
            Traverse.Create(typeof(Time)).Property("fixedUnscaledTime");

        private static void Postfix(ref float __result)
        {
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return;
            // When called from inside MonoBehaviour's FixedUpdate, it returns Time.fixedUnscaledTime
            var gameTime = VirtualEnvironmentFactory.GetVirtualEnv().GameTime;
            __result = inFixedTimeStep.PropertyExists() && inFixedTimeStep.GetValue<bool>()
                ? fixedUnscaledTime.GetValue<float>()
                : (float)(__result - gameTime.UnscaledTimeOffset);
        }
    }

    [HarmonyPatch(typeof(Time), "unscaledTimeAsDouble", MethodType.Getter)]
    private class get_unscaledTimeAsDouble
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static readonly Traverse
            inFixedTimeStep = Traverse.Create(typeof(Time)).Property("inFixedTimeStep");

        private static readonly Traverse fixedUnscaledTimeAsDouble =
            Traverse.Create(typeof(Time)).Property("fixedUnscaledTimeAsDouble");

        private static void Postfix(ref double __result)
        {
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return;
            // When called from inside MonoBehaviour's FixedUpdate, it returns Time.fixedUnscaledTimeAsDouble
            if (inFixedTimeStep.PropertyExists() && inFixedTimeStep.GetValue<bool>())
                __result = fixedUnscaledTimeAsDouble.GetValue<double>();
            else
            {
                var gameTime = VirtualEnvironmentFactory.GetVirtualEnv().GameTime;
                __result -= gameTime.UnscaledTimeOffset;
            }
        }
    }

    [HarmonyPatch(typeof(Time), "fixedUnscaledTimeAsDouble", MethodType.Getter)]
    private class get_fixedUnscaledTimeAsDouble
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static void Postfix(ref double __result)
        {
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return;
            var gameTime = VirtualEnvironmentFactory.GetVirtualEnv().GameTime;
            __result -= gameTime.FixedUnscaledTimeOffset;
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.frameCount), MethodType.Getter)]
    private class get_frameCount
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static void Postfix(ref int __result)
        {
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return;
            var gameTime = VirtualEnvironmentFactory.GetVirtualEnv().GameTime;
            __result = (int)((ulong)__result - gameTime.FrameCountRestartOffset);
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.renderedFrameCount), MethodType.Getter)]
    private class get_renderedFrameCount
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static void Postfix(ref int __result)
        {
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return;
            var gameTime = VirtualEnvironmentFactory.GetVirtualEnv().GameTime;
            __result = (int)((ulong)__result - gameTime.RenderedFrameCountOffset);
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.realtimeSinceStartup), MethodType.Getter)]
    private class get_realtimeSinceStartup
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static void Postfix(ref float __result)
        {
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return;
            var gameTime = VirtualEnvironmentFactory.GetVirtualEnv().GameTime;
            __result = (float)(__result - gameTime.SecondsSinceStartUpOffset);
        }
    }

    [HarmonyPatch(typeof(Time), "realtimeSinceStartupAsDouble", MethodType.Getter)]
    private class get_realtimeSinceStartupAsDouble
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static void Postfix(ref double __result)
        {
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return;
            var gameTime = VirtualEnvironmentFactory.GetVirtualEnv().GameTime;
            __result -= gameTime.SecondsSinceStartUpOffset;
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.time), MethodType.Getter)]
    private class get_time
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static void Postfix(ref float __result)
        {
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return;
            var gameTime = VirtualEnvironmentFactory.GetVirtualEnv().GameTime;
            __result = (float)(__result - gameTime.ScaledTimeOffset);
        }
    }

    [HarmonyPatch(typeof(Time), "timeAsDouble", MethodType.Getter)]
    private class get_timeAsDouble
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static void Postfix(ref double __result)
        {
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return;
            var gameTime = VirtualEnvironmentFactory.GetVirtualEnv().GameTime;
            __result -= gameTime.ScaledTimeOffset;
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.fixedTime), MethodType.Getter)]
    private class get_fixedTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static void Postfix(ref float __result)
        {
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return;
            var gameTime = VirtualEnvironmentFactory.GetVirtualEnv().GameTime;
            __result = (float)(__result - gameTime.ScaledFixedTimeOffset);
        }
    }

    [HarmonyPatch(typeof(Time), "fixedTimeAsDouble", MethodType.Getter)]
    private class get_fixedTimeAsDouble
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static void Postfix(ref double __result)
        {
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return;
            var gameTime = VirtualEnvironmentFactory.GetVirtualEnv().GameTime;
            __result -= gameTime.ScaledFixedTimeOffset;
        }
    }
}