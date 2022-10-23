using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace UniTASPlugin.Patches.UnityEngine;

[HarmonyPatch(typeof(Application), "LoadLevelAsync")]
class LoadLevelAsync
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(Application), "LoadLevelAsync", new global::System.Type[] { typeof(string), typeof(int), typeof(bool), typeof(bool) });
    }

    static global::System.Exception Cleanup(MethodBase original, global::System.Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Prefix(ref bool mustCompleteNextFrame)
    {
        mustCompleteNextFrame = true;
    }
}