using HarmonyLib;
using UniTASPlugin.TAS;
using UnityEngine;

namespace UniTASPlugin.Patches.RNG.__UnityEngine;

[HarmonyPatch(typeof(Random), nameof(Random.InitState))]
class InitState
{
    static void Prefix(ref int seed)
    {
        if (TASTool.Running)
        {
            seed = TASTool.TimeSeed();
            Plugin.Log.LogInfo($"UnityEngine.Random.InitState seed set to {seed}");
        }
    }
}
