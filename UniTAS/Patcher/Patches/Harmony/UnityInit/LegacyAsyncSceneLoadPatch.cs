using System;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Utils;
using UnityEngine;

// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
public static class LegacyAsyncSceneLoadPatch
{
    private static readonly ISceneLoadInvoke SceneLoadInvoke = ContainerStarter.Kernel.GetInstance<ISceneLoadInvoke>();

    private static readonly IPatchReverseInvoker PatchReverseInvoker =
        ContainerStarter.Kernel.GetInstance<IPatchReverseInvoker>();

    [HarmonyPatch]
    private class LoadLevelAsync
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(Application), nameof(Application.LoadLevelAsync),
                [typeof(string), typeof(int), typeof(bool), typeof(bool)]);
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix(ref bool mustCompleteNextFrame)
        {
            if (!PatchReverseInvoker.Invoking)
                mustCompleteNextFrame = true;
            SceneLoadInvoke.SceneLoadCall();
        }
    }
}