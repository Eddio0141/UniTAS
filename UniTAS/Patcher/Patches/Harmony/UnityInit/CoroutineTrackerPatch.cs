using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
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
public class CoroutineTrackerPatch
{
    private static readonly ICoroutineTracker Tracker =
        ContainerStarter.Kernel.GetInstance<ICoroutineTracker>();

    [HarmonyPatch(typeof(MonoBehaviour), nameof(MonoBehaviour.StartCoroutine), typeof(IEnumerator))]
    private class StartCoroutine_IEnumerator
    {
        private static Exception Cleanup(MethodBase original, Exception ex) =>
            PatchHelper.CleanupIgnoreFail(original, ex);

        private static void Prefix(MonoBehaviour __instance, IEnumerator routine)
        {
            Tracker.NewCoroutine(__instance, routine);
        }
    }

    [HarmonyPatch(typeof(MonoBehaviour), nameof(MonoBehaviour.StartCoroutine), typeof(string), typeof(object))]
    private class StartCoroutine_string_object
    {
        private static Exception Cleanup(MethodBase original, Exception ex) =>
            PatchHelper.CleanupIgnoreFail(original, ex);

        private static void Prefix(MonoBehaviour __instance, string methodName, object value)
        {
            Tracker.NewCoroutine(__instance, methodName, value);
        }
    }
}