using Core;
using HarmonyLib;
using System;

namespace v2021_2_14.Patches.RNG.__System;

[HarmonyPatch(typeof(Random), "GenerateSeed")]
class GenerateSeed
{
    static void Postfix(ref int __result)
    {
        Log.LogDebug($"System.Random.GenerateSeed seed: {__result}");
    }
}

[HarmonyPatch(typeof(Random), "GenerateGlobalSeed")]
class GenerateGlobalSeed
{
    static void Postfix(ref int __result)
    {
        Log.LogDebug($"System.Random.GenerateGlobalSeed seed: {__result}");
    }
}
