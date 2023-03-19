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

    [HarmonyPatch(typeof(UnityEngine.Random), nameof(UnityEngine.Random.RandomRangeInt), typeof(int), typeof(int))]
    private class TestPrint
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Postfix(int minInclusive, int maxExclusive, int __result)
        {
            Trace.Write($"UnityEngine.Random.RandomRangeInt({minInclusive}, {maxExclusive}) -> {__result}");
            Trace.Write($"Invoked from {new StackTrace()}");
        }
    }

    [HarmonyPatch(typeof(UnityEngine.Random), nameof(UnityEngine.Random.Range), typeof(float), typeof(float))]
    private class TestPrint2
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Postfix(float minInclusive, float maxInclusive, float __result)
        {
            Trace.Write($"UnityEngine.Random.Range({minInclusive}, {maxInclusive}) -> {__result}");
            Trace.Write($"Invoked from {new StackTrace()}");
        }
    }
}