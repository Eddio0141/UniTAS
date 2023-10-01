using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Patches.Harmony;

[RawPatch]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class ScriptableObjectPatch
{
    private static readonly INewScriptableObjectTracker NewScriptableObjectTracker =
        ContainerStarter.Kernel.GetInstance<INewScriptableObjectTracker>();

    [HarmonyPatch]
    private class CreateInstanceTracker
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Postfix(ScriptableObject __result)
        {
            NewScriptableObjectTracker.NewScriptableObject(__result);
        }
    }
}