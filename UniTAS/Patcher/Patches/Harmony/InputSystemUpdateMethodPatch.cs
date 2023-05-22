using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Shared;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Patches.Harmony;

[HarmonyPatch]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class InputSystemUpdateMethodPatch
{
    [HarmonyPatch]
    private class UpdateModeSetter
    {
        private static void Prefix(InputSettings.UpdateMode value)
        {
            InputSystemEvents.InputSystemChangeUpdate(value);
        }

        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertySetter(typeof(InputSettings), nameof(InputSettings.updateMode));
        }

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        private static bool Prepare()
        {
            return NewInputSystemState.NewInputSystemExists;
        }
    }
}