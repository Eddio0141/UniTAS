using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Harmony;

// [RawPatch(1000)]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class BepInExLoggingReversePatch
{
    [HarmonyPatch]
    private class ConsoleLogListenerPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            // try find the lambda method in DiskLogListener
            // private instance void '<.ctor>b__16_0'(object o)
            var diskLogListenerLambdas = AccessTools.GetDeclaredMethods(typeof(DiskLogListener)).Where(m =>
                m.IsPrivate && !m.IsStatic && m.ReturnType == typeof(void) && m.GetParameters().Length == 1 &&
                m.GetParameters()[0].ParameterType == typeof(object)).ToList();
            if (diskLogListenerLambdas.Count == 1)
            {
                yield return diskLogListenerLambdas[0];
            }
            else
            {
                // further filter by name
                var diskLogListenerLambda = diskLogListenerLambdas.FirstOrDefault(m => m.Name == "<.ctor>b__16_0");
                if (diskLogListenerLambda != null)
                {
                    yield return diskLogListenerLambda;
                }
                else
                {
                    yield return diskLogListenerLambdas.FirstOrDefault(m => m.Name.StartsWith("<.ctor>b__"));
                }
            }

            yield return AccessTools.Method(typeof(DiskLogListener), nameof(DiskLogListener.LogEvent));
            yield return AccessTools.Method(typeof(ConsoleLogListener), nameof(ConsoleLogListener.LogEvent));
            yield return AccessTools.Method(typeof(DiskLogListener), nameof(DiskLogListener.Dispose));
            yield return AccessTools.Method(typeof(UnityLogListener), nameof(UnityLogListener.LogEvent));
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        // private static void Prefix()
        // {
        //     ReverseInvokerFactory.GetReverseInvoker().Invoking = true;
        // }
        //
        // private static void Postfix()
        // {
        //     ReverseInvokerFactory.GetReverseInvoker().Invoking = false;
        // }
    }
}