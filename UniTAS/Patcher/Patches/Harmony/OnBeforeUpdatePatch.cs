using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Harmony;

[RawPatchUnityInit]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class OnBeforeUpdatePatch
{
    private static readonly IMonoBehEventInvoker MonoBehEventInvoker =
        ContainerStarter.Kernel.GetInstance<IMonoBehEventInvoker>();

    private static readonly IMonoBehaviourController MonoBehaviourController =
        ContainerStarter.Kernel.GetInstance<IMonoBehaviourController>();

    [HarmonyPatch]
    private class NotifyBeforeUpdate
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static MethodBase TargetMethod() =>
            AccessTools.Method("UnityEngineInternal.Input.NativeInputSystem:NotifyBeforeUpdate");

        private static bool Prefix(int updateType)
        {
            /*
                Dynamic = 1,
                Fixed = 2,
                BeforeRender = 4,
                IgnoreFocus = -2147483648, // 0x80000000
             */
            if ((updateType & 1) != 0)
            {
                MonoBehEventInvoker.InvokeUpdate();
            }
            else if ((updateType & 2) != 0)
            {
                MonoBehEventInvoker.InvokeFixedUpdate();
            }

            return !MonoBehaviourController.PausedExecution;
        }
    }

    [HarmonyPatch]
    private class NotifyUpdate
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static MethodBase TargetMethod() =>
            AccessTools.Method("UnityEngineInternal.Input.NativeInputSystem:NotifyUpdate");

        private static bool Prefix()
        {
            return !MonoBehaviourController.PausedExecution;
        }
    }
}