using System;
using System.Reflection;
using HarmonyLib;

namespace UniTASPlugin.ReversePatches.__UnityEngine;

internal class Helper
{
    public static Type CursorType()
    {
        return AccessTools.TypeByName("UnityEngine.Cursor");
    }
}

[HarmonyPatch]
public static class Cursor
{
    public static bool visible { get => get_visiblePatch.get(); set => set_visiblePatch.set(value); }
    public static int lockState { get => get_lockStatePatch.get(); set => set_lockStatePatch.set(value); }

    [HarmonyPatch]
    private static class set_visiblePatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertySetter(Helper.CursorType(), "visible");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        public static void set(bool value)
        {
            throw new NotImplementedException();
        }
    }

    [HarmonyPatch]
    private static class get_visiblePatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(Helper.CursorType(), "visible");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        public static bool get()
        {
            throw new NotImplementedException();
        }
    }

    [HarmonyPatch]
    private static class get_lockStatePatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(Helper.CursorType(), "lockState");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        public static int get()
        {
            throw new NotImplementedException();
        }
    }

    [HarmonyPatch]
    private static class set_lockStatePatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertySetter(Helper.CursorType(), "lockState");
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        public static void set(int value)
        {
            throw new NotImplementedException();
        }
    }
}