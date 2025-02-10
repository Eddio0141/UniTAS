using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.MonoBehaviourScripts;
using UniTAS.Patcher.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class ObjectPatch
{
    [HarmonyPatch]
    private class PreventDestruction
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            // Object.Destroy(Object, float)
            // Object.DestroyObject(Object, float)
            // Object.DestroyImmediate(Object, bool)

            return new MethodBase[]
            {
                AccessTools.Method(typeof(Object), nameof(Object.Destroy),
                    [typeof(Object), typeof(float)]),
                AccessTools.Method(typeof(Object), nameof(Object.DestroyObject),
                    [typeof(Object), typeof(float)]),
                AccessTools.Method(typeof(Object), nameof(Object.DestroyImmediate),
                    [typeof(Object), typeof(bool)])
            }.Where(x => x != null);
        }

        private static bool Prefix(Object obj)
        {
            return obj is not MonoBehaviourUpdateInvoker;
        }

        private static Exception Cleanup(MethodBase original, Exception ex) =>
            PatchHelper.CleanupIgnoreFail(original, ex);
    }

    [HarmonyPatch]
    private class PreventInstantiation
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            // Internal_InstantiateSingle_Injected(Object, Vector3, Quaternion)
            // Internal_InstantiateSingleWithParent_Injected(Object, Transform, Vector3, Quaternion)
            // Internal_CloneSingle(Object)
            // Internal_CloneSingleWithParent(Object, Transform, bool)

            return new MethodBase[]
            {
                AccessTools.Method(typeof(Object), "Internal_InstantiateSingle_Injected",
                    [typeof(Object), typeof(Vector3), typeof(Quaternion)]),
                AccessTools.Method(typeof(Object), "Internal_InstantiateSingleWithParent_Injected",
                    [typeof(Object), typeof(Transform), typeof(Vector3), typeof(Quaternion)]),
                AccessTools.Method(typeof(Object), "Internal_CloneSingle",
                    [typeof(Object)]),
                AccessTools.Method(typeof(Object), "Internal_CloneSingleWithParent",
                    [typeof(Object), typeof(Transform), typeof(bool)])
            }.Where(x => x != null);
        }

        private static bool Prefix(Object data)
        {
            return data is not MonoBehaviourUpdateInvoker;
        }

        private static Exception Cleanup(MethodBase original, Exception ex) =>
            PatchHelper.CleanupIgnoreFail(original, ex);
    }
}