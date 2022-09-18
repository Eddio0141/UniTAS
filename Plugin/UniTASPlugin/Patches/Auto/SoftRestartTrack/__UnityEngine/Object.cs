using HarmonyLib;
using UnityEngine;

namespace UniTASPlugin.Patches.Auto.SoftRestartTrack.__UnityEngine;

[HarmonyPatch(typeof(Object), nameof(Object.DontDestroyOnLoad))]
class DontDestroyOnLoad
{
    static void Prefix(Object target)
    {
        if (target == null)
            return;

        var id = target.GetInstanceID();
        if (!Core.TAS.Main.DontDestroyOnLoadIDs.Contains(id))
        {
            Core.TAS.Main.DontDestroyOnLoadIDs.Add(id);
        }
    }
}

[HarmonyPatch(typeof(Object), nameof(Object.Destroy), new System.Type[] { typeof(Object), typeof(float) })]
class Destroy__Object__float
{
    static void Prefix(Object obj)
    {
        if (obj == null)
            return;

        var id = obj.GetInstanceID();

        Core.TAS.Main.DontDestroyOnLoadIDs.Remove(id);
    }
}

[HarmonyPatch(typeof(Object), nameof(Object.DestroyImmediate), new System.Type[] { typeof(Object), typeof(bool) })]
class DestroyImmediate__Object__bool
{
    static void Prefix(Object obj)
    {
        if (obj == null)
            return;

        var id = obj.GetInstanceID();

        Core.TAS.Main.DontDestroyOnLoadIDs.Remove(id);
    }
}