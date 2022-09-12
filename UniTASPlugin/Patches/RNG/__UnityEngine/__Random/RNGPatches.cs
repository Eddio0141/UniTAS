using HarmonyLib;
using UniTASPlugin.TAS;
using UnityEngine;

namespace UniTASPlugin.Patches.TASInput.__UnityEngine.__Random;

[HarmonyPatch(typeof(Random), nameof(Random.InitState))]
class InitState
{
    static void Prefix(ref int seed)
    {
        if (TASTool.Running)
            seed = TASTool.TimeSeed();
    }
}
