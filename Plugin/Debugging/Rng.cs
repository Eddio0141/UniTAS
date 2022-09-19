using HarmonyLib;
using UnityEngine;

namespace UniTASPlugin.Debugging;

[HarmonyPatch(typeof(Random), nameof(Random.Range), new System.Type[] { typeof(int), typeof(int) })]
class Range
{
    static void Postfix(ref int __result)
    {
        Plugin.Log.LogDebug($"Random.Range = {__result}");
    }
}

[HarmonyPatch(typeof(Random), nameof(Random.Range), new System.Type[] { typeof(float), typeof(float) })]
class Rangefloat
{
    static void Postfix(ref float __result)
    {
        Plugin.Log.LogDebug($"Random.Range = {__result}");
    }
}

[HarmonyPatch(typeof(Random), "InitState")]
class Initstate
{
    static void Prefix(int seed)
    {
        Plugin.Log.LogDebug($"Random.InitState({seed})");
    }
}