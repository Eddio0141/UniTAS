using HarmonyLib;
using System;
using System.Reflection;
using TimeOrig = UnityEngine.Time;

namespace UniTASPlugin.ReversePatches.__UnityEngine;

[HarmonyPatch]
public static class Time
{
    [HarmonyPatch]
    public static class captureFramerate
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(TimeOrig), nameof(TimeOrig.captureFramerate), MethodType.Setter)]
        public static void set(int value)
        {
            throw new NotImplementedException();
        }
    }

    [HarmonyPatch]
    public static class captureDeltaTime
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(TimeOrig), "captureDeltaTime", MethodType.Setter)]
        public static void set(float value)
        {
            throw new NotImplementedException();
        }
    }
}
