using HarmonyLib;
using UnityEngine;

namespace UniTASPlugin.Patches.Manual.TASInput.__UnityEngine;

[HarmonyPatch(typeof(Input), nameof(Input.mainGyroIndex_Internal))]
class mainGyroIndex_Internal
{
    static bool Prefix(ref int __result)
    {
        if (TAS.Main.Running)
        {
            // TODO
            return false;
        }
        return true;
    }
}