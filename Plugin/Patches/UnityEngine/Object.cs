using System;
using System.Reflection;
using HarmonyLib;
using ObjectOrig = UnityEngine.Object;
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

namespace UniTASPlugin.Patches.UnityEngine;

[HarmonyPatch]
internal static class Object
{
    [HarmonyPatch(typeof(ObjectOrig), nameof(ObjectOrig.DontDestroyOnLoad))]
    internal class DontDestroyOnLoad
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static void Prefix(ObjectOrig target)
        {
            GameTracker.DontDestroyOnLoadCall(target);
        }
    }

    [HarmonyPatch(typeof(ObjectOrig), nameof(ObjectOrig.Destroy), typeof(ObjectOrig), typeof(float))]
    internal class Destroy__Object__float
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static void Prefix(ObjectOrig obj)
        {
            GameTracker.DestroyObject(obj);
        }
    }

    [HarmonyPatch(typeof(ObjectOrig), nameof(ObjectOrig.DestroyImmediate), typeof(ObjectOrig), typeof(bool))]
    internal class DestroyImmediate__Object__bool
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static void Prefix(ObjectOrig obj)
        {
            GameTracker.DestroyObject(obj);
        }
    }
}