using HarmonyLib;
using System;
using System.Reflection;

namespace UniTASPlugin.ReversePatches.__UnityEngine;

class Helper
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
    static class set_visiblePatch
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.PropertySetter(Helper.CursorType(), "visible");
        }

        static Exception Cleanup(MethodBase original, Exception ex)
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
    static class get_visiblePatch
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(Helper.CursorType(), "visible");
        }

        static Exception Cleanup(MethodBase original, Exception ex)
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
    static class get_lockStatePatch
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(Helper.CursorType(), "lockState");
        }

        static Exception Cleanup(MethodBase original, Exception ex)
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
    static class set_lockStatePatch
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.PropertySetter(Helper.CursorType(), "lockState");
        }

        static Exception Cleanup(MethodBase original, Exception ex)
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