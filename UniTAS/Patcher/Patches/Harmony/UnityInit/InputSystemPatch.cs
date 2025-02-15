using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Utils;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class InputSystemPatch
{
    private static readonly IInputSystemState NewInputSystemState =
        ContainerStarter.Kernel.GetInstance<IInputSystemState>();

    private static readonly IInputSystemTrackerUpdate InputSystemTrackerUpdate =
        ContainerStarter.Kernel.GetInstance<IInputSystemTrackerUpdate>();

    [HarmonyPatch]
    private class add_onBeforeUpdate
    {
        private static bool Prepare()
        {
            return NewInputSystemState.HasNewInputSystem;
        }

        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(InputSystem), $"add_{nameof(InputSystem.onBeforeUpdate)}");
        }

        private static void Prefix(Action value) => InputSystemTrackerUpdate.NewOnBeforeUpdateEvent(value);
    }
}