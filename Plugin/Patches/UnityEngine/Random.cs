/*using HarmonyLib;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine;

[HarmonyPatch(typeof(Random), nameof(Random.Range), new System.Type[] { typeof(float), typeof(float) })]
class RangeFloat
{
    static System.Exception Cleanup(System.Reflection.MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Postfix(float minInclusive, float maxInclusive, ref float __result)
    {
        if (TAS.Main.Running)
        {
            System.Diagnostics.StackTrace trace = new();
            Plugin.Instance.Log.LogDebug($"Random.Range({minInclusive}, {maxInclusive}) returned {__result}, trace: {trace}");
        }
    }
}

[HarmonyPatch(typeof(Random), nameof(Random.Range), new System.Type[] { typeof(int), typeof(int) })]
class RangeInt
{
    static System.Exception Cleanup(System.Reflection.MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Postfix(int minInclusive, int maxExclusive, ref int __result)
    {
        if (TAS.Main.Running)
        {
            System.Diagnostics.StackTrace trace = new();
            Plugin.Instance.Log.LogDebug($"Random.Range({minInclusive}, {maxExclusive}) returned {__result}, trace: {trace}");
        }
    }
}*/