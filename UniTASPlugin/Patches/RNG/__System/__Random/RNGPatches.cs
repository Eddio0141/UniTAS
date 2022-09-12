using HarmonyLib;
using System;
using UniTASPlugin.TAS;

namespace UniTASPlugin.Patches.RNG.__System.__Random;

[HarmonyPatch(typeof(Random), MethodType.Constructor, new Type[] { typeof(int) })]
class Random__Seed
{
    static void Prefix(ref int Seed)
    {
        if (TASTool.Running)
            Seed = TASTool.TimeSeed();
    }
}

[HarmonyPatch(typeof(Random), "GenerateSeed")]
class GenerateSeed
{
    static bool Prefix(ref int __result)
    {
        if (TASTool.Running)
        {
            __result = TASTool.TimeSeed();
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Random), "GenerateGlobalSeed")]
class GenerateGlobalSeed
{
    static bool Prefix(ref int __result)
    {
        if (TASTool.Running)
        {
            __result = TASTool.TimeSeed();
            return false;
        }

        return true;
    }
}
