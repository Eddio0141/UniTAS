using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.Patches.PatchTypes;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UniTAS.Plugin.Utils;

namespace UniTAS.Plugin.Patches;

[RawPatch]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "RedundantAssignment")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class SystemRandomPatch
{
    private static readonly IRandomEnv RandomEnv =
        Plugin.Kernel.GetInstance<IRandomEnv>();

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
            Trace.Write($"System.Random.Generate seed returning {__result}");
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
            Trace.Write($"System.Random.GenerateGlobalSeed seed returning {__result}");
            return false;
        }
    }
}