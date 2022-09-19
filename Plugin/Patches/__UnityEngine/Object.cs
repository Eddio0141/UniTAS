using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine;

[HarmonyPatch(typeof(Object), nameof(Object.DontDestroyOnLoad))]
class DontDestroyOnLoad
{
    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Prefix(Object target)
    {
        if (target == null)
            return;

        int id = target.GetInstanceID();
        if (!TAS.Main.DontDestroyOnLoadIDs.Contains(id))
        {
            TAS.Main.DontDestroyOnLoadIDs.Add(id);
        }
    }
}

[HarmonyPatch(typeof(Object), nameof(Object.Destroy), new System.Type[] { typeof(Object), typeof(float) })]
class Destroy__Object__float
{
    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Prefix(Object obj)
    {
        if (obj == null)
            return;

        int id = obj.GetInstanceID();

        TAS.Main.DontDestroyOnLoadIDs.Remove(id);
    }
}

[HarmonyPatch(typeof(Object), nameof(Object.DestroyImmediate), new System.Type[] { typeof(Object), typeof(bool) })]
class DestroyImmediate__Object__bool
{
    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Prefix(Object obj)
    {
        if (obj == null)
            return;

        int id = obj.GetInstanceID();

        TAS.Main.DontDestroyOnLoadIDs.Remove(id);
    }
}