using System;
using System.Reflection;
using HarmonyLib;
using AppOrig = UnityEngine.Application;

// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment

namespace UniTASPlugin.Patches.UnityEngine;

[HarmonyPatch]
internal static class Application
{
    [HarmonyPatch(typeof(AppOrig), "LoadLevelAsync")]
    private class LoadLevelAsync
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(AppOrig), "LoadLevelAsync",
                new[] { typeof(string), typeof(int), typeof(bool), typeof(bool) });
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static void Prefix(ref bool mustCompleteNextFrame)
        {
            mustCompleteNextFrame = true;
        }
    }
}