using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace UniTASPlugin.Patches.UnityEngine;

[HarmonyPatch(typeof(Application), "LoadLevelAsync")]
internal class LoadLevelAsync
{
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(Application), "LoadLevelAsync", new[] { typeof(string), typeof(int), typeof(bool), typeof(bool) });
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