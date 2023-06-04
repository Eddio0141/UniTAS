using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.MonoBehaviourScripts;
using UniTAS.Patcher.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable UnusedMember.Local

namespace UniTAS.Patcher.Patches.Harmony;

[RawPatch]
// ReSharper disable once ClassNeverInstantiated.Global
public class ObjectPatch
{
    [HarmonyPatch]
    private class PreventPluginDestruction
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            // Object.Destroy(Object, float)
            // Object.DestroyObject(Object, float)
            // Object.DestroyImmediate(Object, bool)

            return new MethodBase[]
            {
                AccessTools.Method(typeof(Object), nameof(Object.Destroy),
                    new[] { typeof(Object), typeof(float) }),
                AccessTools.Method(typeof(Object), nameof(Object.DestroyObject),
                    new[] { typeof(Object), typeof(float) }),
                AccessTools.Method(typeof(Object), nameof(Object.DestroyImmediate),
                    new[] { typeof(Object), typeof(bool) })
            }.Where(x => x != null);
        }

        private static bool Prefix(Object obj)
        {
            // Don't destroy Plugin
            return obj is not MonoBehaviourUpdateInvoker;
        }

        // ReSharper disable once UnusedParameter.Local
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            if (ex != null)
            {
                StaticLogger.Log.LogWarning("Failed to patch Object destruction methods, Plugin may be destroyed");
            }

            return null;
        }
    }

    [HarmonyPatch]
    private class PreventPluginInstantiation
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
                    new[] { typeof(Object), typeof(Vector3), typeof(Quaternion) }),
                AccessTools.Method(typeof(Object), "Internal_InstantiateSingleWithParent_Injected",
                    new[] { typeof(Object), typeof(Transform), typeof(Vector3), typeof(Quaternion) }),
                AccessTools.Method(typeof(Object), "Internal_CloneSingle",
                    new[] { typeof(Object) }),
                AccessTools.Method(typeof(Object), "Internal_CloneSingleWithParent",
                    new[] { typeof(Object), typeof(Transform), typeof(bool) })
            }.Where(x => x != null);
        }

        private static bool Prefix(Object data)
        {
            // Don't instantiate Plugin
            return data is not MonoBehaviourUpdateInvoker;
        }

        // ReSharper disable once UnusedParameter.Local
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            if (ex != null)
            {
                StaticLogger.Log.LogWarning("Failed to patch Object instantiation methods, Plugin may be instantiated");
            }

            return null;
        }
    }
}