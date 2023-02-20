using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.GameEnvironment;
using UniTAS.Plugin.Patches.PatchTypes;
using UniTAS.Plugin.ReverseInvoker;
using UnityEngine;

namespace UniTAS.Plugin.Patches.RawPatches;

[RawPatch]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class TimePatch
{
    private static readonly IPatchReverseInvoker
        ReverseInvoker = Plugin.Kernel.GetInstance<IPatchReverseInvoker>();

    private static readonly VirtualEnvironment VirtualEnvironment =
        Plugin.Kernel.GetInstance<VirtualEnvironment>();

    [HarmonyPatch(typeof(Time), nameof(Time.captureFramerate), MethodType.Setter)]
    private class set_captureFramerate
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix()
        {
            return ReverseInvoker.InnerCall();
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(Time), "captureDeltaTime", MethodType.Setter)]
    private class set_captureDeltaTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix()
        {
            return ReverseInvoker.InnerCall();
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(Time), "fixedUnscaledTime", MethodType.Getter)]
    private class get_fixedUnscaledTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Postfix(ref float __result)
        {
            var gameTime = VirtualEnvironment.GameTime;
            __result = (float)(__result - gameTime.FixedUnscaledTimeOffset);
        }
    }

    [HarmonyPatch(typeof(Time), "unscaledTime", MethodType.Getter)]
    private class get_unscaledTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static readonly Traverse
            inFixedTimeStep = Traverse.Create(typeof(Time)).Property("inFixedTimeStep");

        private static readonly Traverse fixedUnscaledTime =
            Traverse.Create(typeof(Time)).Property("fixedUnscaledTime");

        private static void Postfix(ref float __result)
        {
            // When called from inside MonoBehaviour's FixedUpdate, it returns Time.fixedUnscaledTime
            var gameTime = VirtualEnvironment.GameTime;
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
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static readonly Traverse
            inFixedTimeStep = Traverse.Create(typeof(Time)).Property("inFixedTimeStep");

        private static readonly Traverse fixedUnscaledTimeAsDouble =
            Traverse.Create(typeof(Time)).Property("fixedUnscaledTimeAsDouble");

        private static void Postfix(ref double __result)
        {
            // When called from inside MonoBehaviour's FixedUpdate, it returns Time.fixedUnscaledTimeAsDouble
            if (inFixedTimeStep.PropertyExists() && inFixedTimeStep.GetValue<bool>())
                __result = fixedUnscaledTimeAsDouble.GetValue<double>();
            else
            {
                var gameTime = VirtualEnvironment.GameTime;
                __result -= gameTime.UnscaledTimeOffset;
            }
        }
    }

    [HarmonyPatch(typeof(Time), "fixedUnscaledTimeAsDouble", MethodType.Getter)]
    private class get_fixedUnscaledTimeAsDouble
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Postfix(ref double __result)
        {
            var gameTime = VirtualEnvironment.GameTime;
            __result -= gameTime.FixedUnscaledTimeOffset;
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.frameCount), MethodType.Getter)]
    private class get_frameCount
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Postfix(ref int __result)
        {
            var gameTime = VirtualEnvironment.GameTime;
            __result = (int)((ulong)__result - gameTime.FrameCountRestartOffset);
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.renderedFrameCount), MethodType.Getter)]
    private class get_renderedFrameCount
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Postfix(ref int __result)
        {
            var gameTime = VirtualEnvironment.GameTime;
            __result = (int)((ulong)__result - gameTime.RenderedFrameCountOffset);
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.realtimeSinceStartup), MethodType.Getter)]
    private class get_realtimeSinceStartup
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Postfix(ref float __result)
        {
            var gameTime = VirtualEnvironment.GameTime;
            __result = (float)(__result - gameTime.SecondsSinceStartUpOffset);
        }
    }

    [HarmonyPatch(typeof(Time), "realtimeSinceStartupAsDouble", MethodType.Getter)]
    private class get_realtimeSinceStartupAsDouble
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Postfix(ref double __result)
        {
            var gameTime = VirtualEnvironment.GameTime;
            __result -= gameTime.SecondsSinceStartUpOffset;
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.time), MethodType.Getter)]
    private class get_time
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Postfix(ref float __result)
        {
            var gameTime = VirtualEnvironment.GameTime;
            __result = (float)(__result - gameTime.ScaledTimeOffset);
        }
    }

    [HarmonyPatch(typeof(Time), "timeAsDouble", MethodType.Getter)]
    private class get_timeAsDouble
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Postfix(ref double __result)
        {
            var gameTime = VirtualEnvironment.GameTime;
            __result -= gameTime.ScaledTimeOffset;
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.fixedTime), MethodType.Getter)]
    private class get_fixedTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Postfix(ref float __result)
        {
            var gameTime = VirtualEnvironment.GameTime;
            __result = (float)(__result - gameTime.ScaledFixedTimeOffset);
        }
    }

    [HarmonyPatch(typeof(Time), "fixedTimeAsDouble", MethodType.Getter)]
    private class get_fixedTimeAsDouble
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Postfix(ref double __result)
        {
            var gameTime = VirtualEnvironment.GameTime;
            __result -= gameTime.ScaledFixedTimeOffset;
        }
    }
}