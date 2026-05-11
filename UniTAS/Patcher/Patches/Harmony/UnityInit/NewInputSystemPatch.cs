using System;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
public class NewInputSystemPatch
{
    private static readonly IMonoBehaviourController MonoBehaviourController = ContainerStarter.Kernel.GetInstance<IMonoBehaviourController>();

    private static readonly IInputSystemState NewInputSystemState = ContainerStarter.Kernel.GetInstance<IInputSystemState>();

    private static readonly IPatchReverseInvoker ReverseInvoker = ContainerStarter.Kernel.GetInstance<IPatchReverseInvoker>();
    private static readonly IVirtualEnvController VEnv = ContainerStarter.Kernel.GetInstance<IVirtualEnvController>();

    private static readonly ITimeEnv TimeEnv = ContainerStarter.Kernel.GetInstance<ITimeEnv>();

    private static readonly Type NativeInputSystem = AccessTools.TypeByName("UnityEngineInternal.Input.NativeInputSystem");

    // [HarmonyPatch]
    // TODO: unsure if this is even needed, check
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

        private static bool Prepare()
        {
            return NewInputSystemState.HasNewInputSystem;
        }
    }

    [HarmonyPatch]
    private class get_currentTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex) => PatchHelper.CleanupIgnoreFail(original, ex);

        private static MethodBase TargetMethod() => AccessTools.PropertyGetter(NativeInputSystem, "currentTime");

        private static bool Prefix(ref double __result)
        {
            if (ReverseInvoker.Invoking)
            {
                return true;
            }

            __result = TimeEnv.UnscaledTime;
            return false;
        }
    }

    [HarmonyPatch]
    private class get_currentTimeOffsetToRealtimeSinceStartup
    {
        private static Exception Cleanup(MethodBase original, Exception ex) => PatchHelper.CleanupIgnoreFail(original, ex);

        private static MethodBase TargetMethod() => AccessTools.PropertyGetter(NativeInputSystem, "currentTimeOffsetToRealtimeSinceStartup");

        private static bool Prefix(ref double __result)
        {
            if (ReverseInvoker.Invoking)
            {
                return true;
            }

            __result = 0.0;
            return false;
        }
    }
}
