using HarmonyLib;
using System;
using UniTASPlugin.TAS;

namespace UniTASPlugin.Patches.RNG.__System;

[HarmonyPatch(typeof(Random), "GenerateSeed")]
class GenerateSeed
{
    // TODO do we need to do anything here? seed depends on time, so it should be fine
    /*
    static bool Prefix(ref int __result)
    {
        if (Main.Running)
        {
            __result = Main.TimeSeed();
            return false;
        }
        return true;
    }
    */

    static void Postfix(ref int __result)
    {
        Plugin.Log.LogInfo($"System.Random.GenerateSeed seed: {__result}");
    }
}

[HarmonyPatch(typeof(Random), "GenerateGlobalSeed")]
class GenerateGlobalSeed
{
    // TODO do we need to do anything here? seed depends on time, so it should be fine
    /*
    static bool Prefix(ref int __result)
    {
        if (Main.Running)
        {
            __result = Main.TimeSeed();
            return false;
        }
        return true;
    }
    */

    static void Postfix(ref int __result)
    {
        Plugin.Log.LogInfo($"System.Random.GenerateGlobalSeed seed: {__result}");
    }
}
