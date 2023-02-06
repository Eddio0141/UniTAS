using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.Interfaces;
using UniTASPlugin.ReverseInvoker;
using UnityEngine;

// ReSharper disable UnusedMember.Local

namespace UniTASPlugin.Patches.RawPatchOnPluginInit;

[PatchTypes.RawPatchOnPluginInit]
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
        var rev = ReverseInvokerFactory.GetReverseInvoker();
        rev.Invoking = true;
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

        rev.Invoking = false;
    }

    private static readonly IMonoBehEventInvoker
        MonoBehEventInvoker = Plugin.Kernel.GetInstance<IMonoBehEventInvoker>();

    private static readonly ReverseInvokerFactory ReverseInvokerFactory =
        Plugin.Kernel.GetInstance<ReverseInvokerFactory>();

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
                Plugin.Log.LogFatal(
                    $"Error patching MonoBehaviour.Awake in all types, closing tool since continuing can cause desyncs: {ex}");
            }

            return ex;
        }
    }

    [HarmonyPatch]
    private class FixedUpdateMultiple
    {
        public static IEnumerable<MethodBase> TargetMethods() => GetEventMethods("FixedUpdate");

        public static void Prefix()
        {
            MonoBehEventInvoker.PreFixedUpdate();
        }

        // ReSharper disable once UnusedParameter.Local
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            if (ex != null)
            {
                Plugin.Log.LogFatal(
                    $"Error patching MonoBehaviour.FixedUpdate in all types, closing tool since continuing can cause desyncs: {ex}");
            }

            return ex;
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
                Plugin.Log.LogFatal(
                    $"Error patching MonoBehaviour.Update in all types, closing tool since continuing can cause desyncs: {ex}");
            }

            return ex;
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
                Plugin.Log.LogFatal(
                    $"Error patching MonoBehaviour.OnEnable in all types, closing tool since continuing can cause desyncs: {ex}");
            }

            return ex;
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
                Plugin.Log.LogFatal(
                    $"Error patching MonoBehaviour.Start in all types, closing tool since continuing can cause desyncs: {ex}");
            }

            return ex;
        }
    }
}