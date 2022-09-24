using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine;

[HarmonyPatch(typeof(Application), "LoadLevelAsync")]
class LoadLevelAsync
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(Application), "LoadLevelAsync", new System.Type[] { typeof(string), typeof(int), typeof(bool), typeof(bool) });
    }

    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Prefix(ref bool mustCompleteNextFrame)
    {
        mustCompleteNextFrame = true;
    }
}