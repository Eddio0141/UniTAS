using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.Patches.PatchTypes;
using UniTAS.Plugin.Services.VirtualEnvironment;

namespace UniTAS.Plugin.Patches.RawPatches;

[RawPatch]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "RedundantAssignment")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class SystemRandomPatch
{
    private static readonly VirtualEnvironment VirtualEnvironment =
        Plugin.Kernel.GetInstance<VirtualEnvironment>();

    [HarmonyPatch(typeof(Random), "GenerateSeed")]
    private class GenerateSeed
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            __result = (int)VirtualEnvironment.Seed;
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
            __result = (int)VirtualEnvironment.Seed;
            Trace.Write($"System.Random.GenerateGlobalSeed seed returning {__result}");
            return false;
        }
    }
}