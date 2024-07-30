using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class NewInputSystemPatch
{
    private static readonly IMonoBehaviourController MonoBehaviourController =
        ContainerStarter.Kernel.GetInstance<IMonoBehaviourController>();

    private static readonly IInputSystemState NewInputSystemState =
        ContainerStarter.Kernel.GetInstance<IInputSystemState>();

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
            return NewInputSystemState.HasNewInputSystem;
        }
    }
}
