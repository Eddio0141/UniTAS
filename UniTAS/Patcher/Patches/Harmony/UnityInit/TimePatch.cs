using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "RedundantAssignment")]
public class TimePatch
{
    private static readonly IPatchReverseInvoker
        ReverseInvoker = ContainerStarter.Kernel.GetInstance<IPatchReverseInvoker>();

    private static readonly ITimeEnv TimeEnv = ContainerStarter.Kernel.GetInstance<ITimeEnv>();

    private static bool CalledFromNamespace(string targetNamespace)
    {
        var frames = new StackTrace().GetFrames();
        if (frames == null) return false;

        foreach (var frame in frames)
        {
            var method = frame.GetMethod();

            var declaringNamespace = method?.DeclaringType?.Namespace;
            if (declaringNamespace?.StartsWith(targetNamespace) is true) return true;
        }

        return false;
    }

    private static bool CalledFromFixedUpdate()
    {
        var frames = new StackTrace().GetFrames();
        if (frames == null) return false;

        foreach (var frame in frames)
        {
            var method = frame.GetMethod();
            if (method?.Name is not "FixedUpdate") continue;

            var declType = method.DeclaringType;
            while (declType != null)
            {
                if (declType.IsSubclassOf(typeof(MonoBehaviour))) return true;
                declType = declType.DeclaringType;
            }
        }

        return false;
    }

    [HarmonyPatch(typeof(Time), nameof(Time.captureFramerate), MethodType.Setter)]
    private class set_captureFramerate
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix()
        {
            return ReverseInvoker.Invoking;
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
            return ReverseInvoker.Invoking;
        }
    }

    [HarmonyPatch(typeof(Time), "fixedUnscaledTime", MethodType.Getter)]
    private class get_fixedUnscaledTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref float __result)
        {
            __result = (float)TimeEnv.FixedUnscaledTime;
            return false;
        }
    }

    [HarmonyPatch(typeof(Time), "fixedUnscaledTimeAsDouble", MethodType.Getter)]
    private class get_fixedUnscaledTimeAsDouble
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref double __result)
        {
            __result = TimeEnv.FixedUnscaledTime;
            return false;
        }
    }

    [HarmonyPatch(typeof(Time), "unscaledTime", MethodType.Getter)]
    private class get_unscaledTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref float __result)
        {
            // When called from inside MonoBehaviour's FixedUpdate, it returns Time.fixedUnscaledTime
            __result = CalledFromFixedUpdate()
                ? (float)TimeEnv.FixedUnscaledTime
                : (float)TimeEnv.UnscaledTime;
            return false;
        }
    }

    [HarmonyPatch(typeof(Time), "unscaledTimeAsDouble", MethodType.Getter)]
    private class get_unscaledTimeAsDouble
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref double __result)
        {
            // When called from inside MonoBehaviour's FixedUpdate, it returns Time.fixedUnscaledTimeAsDouble
            __result = CalledFromFixedUpdate()
                ? TimeEnv.FixedUnscaledTime
                : TimeEnv.UnscaledTime;
            return false;
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
            if (ReverseInvoker.Invoking) return;
            __result = (int)((ulong)__result - TimeEnv.FrameCountRestartOffset);
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.renderedFrameCount), MethodType.Getter)]
    private class get_renderedFrameCount
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool _warnGetterCall;

        private static void Postfix(ref int __result)
        {
            if (ReverseInvoker.Invoking) return;
            // https://discussions.unity.com/t/time-framecount-vs-time-renderedframecount/134435
            // https://web.archive.org/web/20240822132700/https://discussions.unity.com/t/time-framecount-vs-time-renderedframecount/134435
            // some versions may have this weird behaviour
            if (!_warnGetterCall)
            {
                _warnGetterCall = true;
                StaticLogger.LogWarning("get_renderedFrameCount called, behaviour may be inaccurate");
            }

            __result = (int)((ulong)__result - TimeEnv.RenderedFrameCountOffset);
        }
    }

    [HarmonyPatch]
    private class StopwatchGetTimestamp
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            return AccessTools.GetDeclaredMethods(typeof(Stopwatch))
                .Cast<MethodBase>();
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var originalMethod = AccessTools.Method(typeof(Stopwatch), nameof(Stopwatch.GetTimestamp));
            var replacementMethod = AccessTools.Method(typeof(StopwatchGetTimestamp), nameof(GetTimestamp));

            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Call && instruction.operand is MethodInfo method &&
                    method == originalMethod)
                {
                    var repl = new CodeInstruction(OpCodes.Call, replacementMethod);
                    repl.labels.AddRange(instruction.labels);
                    yield return repl;
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        public static long GetTimestamp()
        {
            if (!CalledFromNamespace("Rewired"))
            {
                return Stopwatch.GetTimestamp();
            }

            return (long)(TimeEnv.UnscaledTime * Stopwatch.Frequency);
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.realtimeSinceStartup), MethodType.Getter)]
    private class get_realtimeSinceStartup
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref float __result)
        {
            if (ReverseInvoker.Invoking)
            {
                return true;
            }

            __result = (float)TimeEnv.UnscaledTime;
            return false;
        }
    }

    [HarmonyPatch(typeof(Time), "realtimeSinceStartupAsDouble", MethodType.Getter)]
    private class get_realtimeSinceStartupAsDouble
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref double __result)
        {
            __result = TimeEnv.UnscaledTime;
            return false;
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.time), MethodType.Getter)]
    private class get_time
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref float __result)
        {
            if (ReverseInvoker.Invoking)
                return true;
            __result = CalledFromFixedUpdate() ? (float)TimeEnv.ScaledFixedTime : (float)TimeEnv.ScaledTime;
            return false;
        }
    }

    [HarmonyPatch(typeof(Time), "timeAsDouble", MethodType.Getter)]
    private class get_timeAsDouble
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref double __result)
        {
            __result = CalledFromFixedUpdate()
                ? TimeEnv.ScaledFixedTime
                : TimeEnv.ScaledTime;
            return false;
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.fixedTime), MethodType.Getter)]
    private class get_fixedTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref float __result)
        {
            if (ReverseInvoker.Invoking) return true;
            __result = (float)TimeEnv.ScaledFixedTime;
            return false;
        }
    }

    [HarmonyPatch(typeof(Time), "fixedTimeAsDouble", MethodType.Getter)]
    private class get_fixedTimeAsDouble
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref double __result)
        {
            __result = TimeEnv.ScaledFixedTime;
            return false;
        }
    }

    [HarmonyPatch(typeof(Time), "fixedUnscaledDeltaTime", MethodType.Getter)]
    private class get_fixedUnscaledDeltaTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref float __result)
        {
            __result = Time.fixedDeltaTime;
            return false;
        }
    }

    [HarmonyPatch(typeof(Time), "unscaledDeltaTime", MethodType.Getter)]
    private class get_unscaledDeltaTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref float __result)
        {
            __result = (float)TimeEnv.FrameTime;
            return false;
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.timeSinceLevelLoad), MethodType.Getter)]
    private class get_timeSinceLevelLoad
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref float __result)
        {
            __result = CalledFromFixedUpdate()
                ? (float)TimeEnv.FixedTimeSinceLevelLoad
                : (float)TimeEnv.TimeSinceLevelLoad;
            return false;
        }
    }

    [HarmonyPatch(typeof(Time), "timeSinceLevelLoadAsDouble", MethodType.Getter)]
    private class get_timeSinceLevelLoadAsDouble
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref double __result)
        {
            __result = CalledFromFixedUpdate() ? TimeEnv.FixedTimeSinceLevelLoad : TimeEnv.TimeSinceLevelLoad;
            return false;
        }
    }
}