using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.LegacyPatches;
using UniTASPlugin.Patches.PatchTypes;
using UnityEngine;

// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment

namespace UniTASPlugin.Patches.RawPatches;

[RawPatch]
public static class LegacyAsyncSceneLoadPatch
{
    [HarmonyPatch(typeof(Application), "LoadLevelAsync")]
    private class LoadLevelAsync
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(Application), nameof(Application.LoadLevelAsync),
                new[] { typeof(string), typeof(int), typeof(bool), typeof(bool) });
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static void Prefix(ref bool mustCompleteNextFrame)
        {
            mustCompleteNextFrame = true;
        }
    }
}