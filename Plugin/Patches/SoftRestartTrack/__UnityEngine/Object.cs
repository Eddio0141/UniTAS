using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace UniTASPlugin.Patches.SoftRestartTrack.__UnityEngine;

[HarmonyPatch(typeof(Object))]
class ObjectPatch
{
    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return Auxilary.Cleanup_IgnoreNotFound(original, ex);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Object.DontDestroyOnLoad))]
    static void Prefix_DontDestroyOnLoad(Object target)
    {
        if (target == null)
            return;

        var id = target.GetInstanceID();
        if (!TAS.Main.DontDestroyOnLoadIDs.Contains(id))
        {
            TAS.Main.DontDestroyOnLoadIDs.Add(id);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Object.Destroy), new System.Type[] { typeof(Object), typeof(float) })]
    static void Prefix_Destroy__Object__float(Object obj)
    {
        if (obj == null)
            return;

        var id = obj.GetInstanceID();

        TAS.Main.DontDestroyOnLoadIDs.Remove(id);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Object.DestroyImmediate), new System.Type[] { typeof(Object), typeof(bool) })]
    static void Prefix_DestroyImmediate__Object__bool(Object obj)
    {
        if (obj == null)
            return;

        var id = obj.GetInstanceID();

        TAS.Main.DontDestroyOnLoadIDs.Remove(id);
    }
}