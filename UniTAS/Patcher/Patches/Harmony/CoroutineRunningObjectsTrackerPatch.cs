using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services.Trackers;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Patches.Harmony;

[RawPatch]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class CoroutineRunningObjectsTrackerPatch
{
    private static readonly ICoroutineRunningObjectsTracker Tracker =
        ContainerStarter.Kernel.GetInstance<ICoroutineRunningObjectsTracker>();

    [HarmonyPatch]
    private class RunCoroutine
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static IEnumerable<MethodBase> TargetMethods()
        {
            // just patch them all, duplicate instances will be detected anyway
            return AccessTools.GetDeclaredMethods(typeof(MonoBehaviour))
                .Where(x => !x.IsStatic && x.Name == "StartCoroutine").Select(x => (MethodBase)x);
        }

        private static void Prefix(MonoBehaviour __instance)
        {
            Tracker.NewCoroutine(__instance);
        }
    }
}