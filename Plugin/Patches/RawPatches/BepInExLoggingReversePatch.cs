using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using UniTASPlugin.Patches.PatchTypes;
using UniTASPlugin.ReverseInvoker;

namespace UniTASPlugin.Patches.RawPatches;

[RawPatch(1000)]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class BepInExLoggingReversePatch
{
    private static readonly IReverseInvokerFactory
        ReverseInvokerFactory = Plugin.Kernel.GetInstance<IReverseInvokerFactory>();

    [HarmonyPatch]
    private class ConsoleLogListenerPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            // ConsoleLogListener.LogEvent
            // DiskLogListener.LogEvent
            // DiskLogListener.Dispose
            // UnityLogListener.LogEvent

            yield return AccessTools.Method(typeof(ConsoleLogListener), nameof(ConsoleLogListener.LogEvent));
            yield return AccessTools.Method(typeof(DiskLogListener), nameof(DiskLogListener.LogEvent));
            yield return AccessTools.Method(typeof(DiskLogListener), nameof(DiskLogListener.Dispose));
            yield return AccessTools.Method(typeof(UnityLogListener), nameof(UnityLogListener.LogEvent));
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix()
        {
            ReverseInvokerFactory.GetReverseInvoker().Invoking = true;
        }

        private static void Postfix()
        {
            ReverseInvokerFactory.GetReverseInvoker().Invoking = false;
        }
    }
}