using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.Interfaces;
using UnityEngine;

// ReSharper disable UnusedMember.Local

namespace UniTAS.Plugin.Patches.RawPatchOnPluginInit;

[PatchTypes.RawPatchOnPluginInit(1000)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
// ReSharper disable once ClassNeverInstantiated.Global
public class MonoBehaviourUpdateInvokePatch
{
    /// <summary>
    /// Finds MonoBehaviour methods which are called on events but doesn't exist in MonoBehaviour itself.
    /// </summary>
    /// <param name="methodName">Event method name</param>
    /// <returns></returns>
    private static IEnumerable<MethodBase> GetEventMethods(string methodName)
    {
        var monoBehaviourTypes = new List<Type>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (!type.IsAbstract && type.IsSubclassOf(typeof(MonoBehaviour)))
                    {
                        monoBehaviourTypes.Add(type);
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                // ignored
            }
        }

        foreach (var monoBehaviourType in monoBehaviourTypes)
        {
            var updateMethod = monoBehaviourType.GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (updateMethod != null)
                yield return updateMethod;
        }
    }

    private static readonly IMonoBehEventInvoker
        MonoBehEventInvoker = Plugin.Kernel.GetInstance<IMonoBehEventInvoker>();

    [HarmonyPatch]
    private class AwakeMultiple
    {
        public static IEnumerable<MethodBase> TargetMethods() => GetEventMethods("Awake");

        public static void Prefix()
        {
            MonoBehEventInvoker.Awake();
        }

        // ReSharper disable once UnusedParameter.Local
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            if (ex != null)
            {
                Plugin.Log.LogWarning(
                    $"Error patching MonoBehaviour.Awake, Method: {original?.DeclaringType?.FullName ?? "unknown_type"}.{original?.Name ?? "unknown_method"}, Exception: {ex}");
            }

            return null;
        }
    }

    [HarmonyPatch]
    private class FixedUpdateMultiple
    {
        public static IEnumerable<MethodBase> TargetMethods() => GetEventMethods("FixedUpdate");

        public static void Prefix()
        {
            MonoBehEventInvoker.FixedUpdate();
        }

        // ReSharper disable once UnusedParameter.Local
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            if (ex != null)
            {
                Plugin.Log.LogWarning(
                    $"Error patching MonoBehaviour.FixedUpdate, Method: {original?.DeclaringType?.FullName ?? "unknown_type"}.{original?.Name ?? "unknown_method"}, Exception: {ex}");
            }

            return null;
        }
    }

    [HarmonyPatch]
    private class UpdateMultiple
    {
        public static IEnumerable<MethodBase> TargetMethods() => GetEventMethods("Update");

        public static void Prefix()
        {
            MonoBehEventInvoker.Update();
        }

        // ReSharper disable once UnusedParameter.Local
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            if (ex != null)
            {
                Plugin.Log.LogWarning(
                    $"Error patching MonoBehaviour.Update, Method: {original?.DeclaringType?.FullName ?? "unknown_type"}.{original?.Name ?? "unknown_method"}, Exception: {ex}");
            }

            return null;
        }
    }

    [HarmonyPatch]
    private class OnEnableMultiple
    {
        public static IEnumerable<MethodBase> TargetMethods() => GetEventMethods("OnEnable");

        public static void Prefix()
        {
            MonoBehEventInvoker.OnEnable();
        }

        // ReSharper disable once UnusedParameter.Local
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            if (ex != null)
            {
                Plugin.Log.LogWarning(
                    $"Error patching MonoBehaviour.OnEnable, Method: {original?.DeclaringType?.FullName ?? "unknown_type"}.{original?.Name ?? "unknown_method"}, Exception: {ex}");
            }

            return null;
        }
    }

    [HarmonyPatch]
    private class StartMultiple
    {
        public static IEnumerable<MethodBase> TargetMethods() => GetEventMethods("Start");

        public static void Prefix()
        {
            MonoBehEventInvoker.Start();
        }

        // ReSharper disable once UnusedParameter.Local
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            if (ex != null)
            {
                Plugin.Log.LogWarning(
                    $"Error patching MonoBehaviour.Start, Method: {original?.DeclaringType?.FullName ?? "unknown_type"}.{original?.Name ?? "unknown_method"}, Exception: {ex}");
            }

            return null;
        }
    }
}