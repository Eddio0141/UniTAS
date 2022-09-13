using HarmonyLib;
using System;
using UniTASPlugin.TAS;

namespace UniTASPlugin.Patches.RNG.__System;

[HarmonyPatch(typeof(Random), MethodType.Constructor, new Type[] { typeof(int) })]
class Random__Seed
{
    static void Prefix(ref int Seed)
    {
        if (TASTool.Running)
        {
            Seed = TASTool.TimeSeed();
            Plugin.Log.LogInfo($"System.Random initialize seed set to {Seed}");
        }
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
            Plugin.Log.LogInfo($"System.Random.GenerateSeed seed set to {__result}");

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
            Plugin.Log.LogInfo($"System.Random.GenerateGlobalSeed seed set to {__result}");

            return false;
        }

        return true;
    }
}
