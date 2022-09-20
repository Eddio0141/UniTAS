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

    static void Postfix(float min, float max, ref float __result)
    {
        if (TAS.Main.Running)
        {
            var trace = new System.Diagnostics.StackTrace();
            Plugin.Log.LogDebug($"Random.Range({min}, {max}) returned {__result}, trace: {trace}");
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

    static void Postfix(int min, int max, ref int __result)
    {
        if (TAS.Main.Running)
        {
            var trace = new System.Diagnostics.StackTrace();
            Plugin.Log.LogDebug($"Random.Range({min}, {max}) returned {__result}, trace: {trace}");
        }
    }
}