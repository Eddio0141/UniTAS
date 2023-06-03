using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.StaticServices;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Patches.Harmony;

[RawPatch]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
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