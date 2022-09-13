using HarmonyLib;
using System;
using UniTASPlugin.TAS;

namespace UniTASPlugin.Patches.RNG.__System;

[HarmonyPatch(typeof(Random), "GenerateSeed")]
class GenerateSeed
{
    static bool Prefix(ref int __result)
    {
        if (Main.Running)
        {
            __result = Main.TimeSeed();
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
        if (Main.Running)
        {
            __result = Main.TimeSeed();
            Plugin.Log.LogInfo($"System.Random.GenerateGlobalSeed seed set to {__result}");

            return false;
        }

        return true;
    }
}
