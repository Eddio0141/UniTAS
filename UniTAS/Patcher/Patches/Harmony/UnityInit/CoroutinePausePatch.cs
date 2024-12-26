using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Utils;
using UnityEngine;
using UniTAS.Patcher.ContainerBindings.GameExecutionControllers;

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "RedundantAssignment")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class CoroutinePausePatch
{
    [HarmonyPatch(typeof(SetupCoroutine), "InvokeMoveNext")]
    private class InvokeMoveNext
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(IEnumerator enumerator)
        {
            if (MonoBehaviourController.IgnoreCoroutines.Contains(enumerator)) return true;
            // TODO: pause update
            return !MonoBehaviourController.PausedExecution;
        }
    }
}