using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Shared;
using UniTAS.Plugin.Interfaces.Patches.PatchTypes;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Utils;

namespace UniTAS.Plugin.Patches;

[RawPatch]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class NewInputSystemPatch
{
    private static readonly IMonoBehaviourController MonoBehaviourController =
        Plugin.Kernel.GetInstance<IMonoBehaviourController>();

    [HarmonyPatch]
    private class SuppressNotifyUpdate
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static MethodBase TargetMethod()
        {
            return AccessTools.Method("UnityEngineInternal.Input.NativeInputSystem:NotifyUpdate");
        }

        private static bool Prefix()
        {
            return !MonoBehaviourController.PausedExecution;
        }

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        private static bool Prepare()
        {
            return NewInputSystemState.NewInputSystemExists;
        }
    }
}