using HarmonyLib;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine;

[HarmonyPatch(typeof(Random), nameof(Random.Range), new System.Type[] { typeof(float), typeof(float) })]
class RangeFloat
{
    static System.Exception Cleanup(System.Reflection.MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Postfix(ref float __result)
    {
        Plugin.Log.LogDebug($"Random.Range(float, float) returned {__result}");
    }
}

[HarmonyPatch(typeof(Random), nameof(Random.Range), new System.Type[] { typeof(int), typeof(int) })]
class RangeInt
{
    static System.Exception Cleanup(System.Reflection.MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Postfix(ref int __result)
    {
        Plugin.Log.LogDebug($"Random.Range(int, int) returned {__result}");
    }
}