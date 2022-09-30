using HarmonyLib;
using System;
using System.Reflection;
using ScreenOrig = UnityEngine.Screen;

namespace UniTASPlugin.ReversePatches.__UnityEngine;

[HarmonyPatch]
public static class Screen
{
    public static bool showCursor { get => showCursorPatch.get(); set => showCursorPatch.set(value); }
    public static bool lockCursor { get => lockCursorPatch.get(); set => lockCursorPatch.set(value); }

    [HarmonyPatch]
    static class showCursorPatch
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ScreenOrig), nameof(ScreenOrig.showCursor), MethodType.Setter)]
        public static void set(bool value)
        {
            throw new NotImplementedException();
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ScreenOrig), nameof(ScreenOrig.showCursor), MethodType.Getter)]
        public static bool get()
        {
            throw new NotImplementedException();
        }
    }

    [HarmonyPatch]
    static class lockCursorPatch
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ScreenOrig), nameof(ScreenOrig.lockCursor), MethodType.Setter)]
        public static void set(bool value)
        {
            throw new NotImplementedException();
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ScreenOrig), nameof(ScreenOrig.lockCursor), MethodType.Getter)]
        public static bool get()
        {
            throw new NotImplementedException();
        }
    }
}