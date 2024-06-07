using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "RedundantAssignment")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class SystemRandomPatch
{
    private static readonly IRandomEnv RandomEnv =
        ContainerStarter.Kernel.GetInstance<IRandomEnv>();

    [HarmonyPatch(typeof(Random), "GenerateSeed")]
    private class GenerateSeed
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            __result = (int)RandomEnv.StartUpSeed;
            return false;
        }
    }

    [HarmonyPatch(typeof(Random), "GenerateGlobalSeed")]
    private class GenerateGlobalSeed
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            __result = (int)RandomEnv.StartUpSeed;
            return false;
        }
    }
}