using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UniTAS.Patcher.Shared;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Patches.Harmony;

[HarmonyPatch]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class InputSystemUpdateMethodPatch
{
    [HarmonyPatch(typeof(InputSettings), nameof(InputSettings.updateMode), MethodType.Setter)]
    private class UpdateModeSetter
    {
        private static void Prefix(InputSettings.UpdateMode value)
        {
            InputSystemEvents.InputSystemChangeUpdate(value);
        }
    }
}