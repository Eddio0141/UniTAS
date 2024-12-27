using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
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
        private static Exception Cleanup(MethodBase original, Exception ex) =>
            PatchHelper.CleanupIgnoreFail(original, ex);

        private static IEnumerable<MethodBase> TargetMethods()
        {
            // just patch them all, duplicate instances will be detected anyway
            return AccessTools.GetDeclaredMethods(typeof(MonoBehaviour))
                .Where(x => !x.IsStatic && x.Name == "StartCoroutine").Select(MethodBase (x) => x);
        }

        private static void Prefix(MonoBehaviour __instance)
        {
            if (Equals(__instance.GetType().Assembly, typeof(CoroutineRunningObjectsTrackerPatch).Assembly)) return;
            Tracker.NewCoroutine(__instance);
        }
    }
}