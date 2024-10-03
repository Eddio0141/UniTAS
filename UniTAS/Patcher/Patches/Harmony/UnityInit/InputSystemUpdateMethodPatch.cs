using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Utils;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class InputSystemUpdateMethodPatch
{
    private static readonly IInputEventInvoker InputEventInvoker =
        ContainerStarter.Kernel.GetInstance<IInputEventInvoker>();

    private static readonly IInputSystemState NewInputSystemState =
        ContainerStarter.Kernel.GetInstance<IInputSystemState>();

    [HarmonyPatch]
    private class UpdateModeSetter
    {
        private static void Prefix(InputSettings.UpdateMode value)
        {
            InputEventInvoker.InputSystemChangeUpdate(value);
        }

        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertySetter(typeof(InputSettings), nameof(InputSettings.updateMode));
        }

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        private static bool Prepare()
        {
            return NewInputSystemState.HasNewInputSystem;
        }
    }
}
