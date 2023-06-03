﻿using System;
using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.Patches.PatchTypes;
using UniTAS.Plugin.Utils;
using UnityEngine;

// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment

namespace UniTAS.Plugin.Patches;

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
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix(ref bool mustCompleteNextFrame)
        {
            mustCompleteNextFrame = true;
        }
    }
}