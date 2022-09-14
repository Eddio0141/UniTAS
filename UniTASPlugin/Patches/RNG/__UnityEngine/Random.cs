using HarmonyLib;
using UnityEngine;

namespace UniTASPlugin.Patches.RNG.__UnityEngine;

[HarmonyPatch(typeof(Random), nameof(Random.InitState))]
class InitState
{
    static void Prefix(ref int seed)
    {
        /*
        if (Main.Running)
        {
            seed = Main.TimeSeed();
        }
        */
        // TODO do we need to do anything here? seed depends on time, so it should be fine

        Plugin.Log.LogInfo($"UnityEngine.Random.InitState seed: {seed}");
    }
}
