using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Trackers;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Patches.Harmony;

[RawPatch]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class ApplicationPatch
{
    private static readonly IRunInBackgroundTracker RunInBackgroundTracker =
        ContainerStarter.Kernel.GetInstance<IRunInBackgroundTracker>();

    private static readonly IPatchReverseInvoker ReverseInvoker =
        ContainerStarter.Kernel.GetInstance<IPatchReverseInvoker>();

    [HarmonyPatch(typeof(Application), nameof(Application.runInBackground), MethodType.Setter)]
    private static class RunInBackgroundSet
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(bool value)
        {
            if (ReverseInvoker.InnerCall()) return true;
            RunInBackgroundTracker.RunInBackground = value;
            return false;
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(Application), nameof(Application.runInBackground), MethodType.Getter)]
    private static class RunInBackgroundGet
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref bool __result)
        {
            if (ReverseInvoker.InnerCall()) return true;
            __result = RunInBackgroundTracker.RunInBackground;
            return false;
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }
}