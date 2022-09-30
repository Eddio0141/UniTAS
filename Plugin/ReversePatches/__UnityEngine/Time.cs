using HarmonyLib;
using System;
using System.Reflection;
using TimeOrig = UnityEngine.Time;

namespace UniTASPlugin.ReversePatches.__UnityEngine;

[HarmonyPatch]
public static class Time
{
    public static int captureFramerate { get => captureFrameratePatch.get(); set => captureFrameratePatch.set(value); }
    public static float captureDeltaTime { get => captureDeltaTimePatch.get(); set => captureDeltaTimePatch.set(value); }

    [HarmonyPatch]
    static class captureFrameratePatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(TimeOrig), nameof(TimeOrig.captureFramerate), MethodType.Getter)]
        public static int get()
        {
            throw new NotImplementedException();
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(TimeOrig), nameof(TimeOrig.captureFramerate), MethodType.Setter)]
        public static void set(int value)
        {
            throw new NotImplementedException();
        }
    }

    [HarmonyPatch]
    static class captureDeltaTimePatch
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(TimeOrig), "captureDeltaTime", MethodType.Getter)]
        public static float get()
        {
            throw new NotImplementedException();
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(TimeOrig), "captureDeltaTime", MethodType.Setter)]
        public static void set(float value)
        {
            throw new NotImplementedException();
        }
    }
}
