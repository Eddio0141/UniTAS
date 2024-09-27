using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class RewiredPatch
{
    [HarmonyPatch]
    private static class InputManagerBaseAwake
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method("Rewired.InputManager_Base:Awake");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix(object __instance)
        {
            new Traverse(__instance).Field("_userData").Field("configVars").Field("alwaysUseUnityInput").SetValue(true);
            StaticLogger.Log.LogInfo("applied fix to rewired input to fallback on unity input");
        }
    }
}