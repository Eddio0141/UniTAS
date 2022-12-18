using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

// ReSharper disable UnusedMember.Local

namespace UniTASPlugin.Patches.UnityEngine;

[HarmonyPatch]
public class MonoBehaviourPatch
{
    private static PluginWrapper pluginWrapper;

    [HarmonyPatch]
    private class UpdateMultiple
    {
        public static IEnumerable<MethodBase> TargetMethods()
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
                var updateMethod = monoBehaviourType.GetMethod("Update",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (updateMethod != null)
                    yield return updateMethod;
            }
        }

        public static void Prefix()
        {
            pluginWrapper ??= Plugin.Kernel.GetInstance<PluginWrapper>();

            pluginWrapper.Update();
        }

        // ReSharper disable once UnusedParameter.Local
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            Plugin.Log.LogError(
                $"Error patching MonoBehaviour.Update in all types, closing tool since continuing can cause desyncs: {ex}");
            return ex;
        }
    }

    [HarmonyPatch]
    private class OnGUIMultiple
    {
        public static IEnumerable<MethodBase> TargetMethods()
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
                var updateMethod = monoBehaviourType.GetMethod("OnGUI",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (updateMethod != null)
                    yield return updateMethod;
            }
        }

        public static void Prefix()
        {
            pluginWrapper ??= Plugin.Kernel.GetInstance<PluginWrapper>();

            pluginWrapper.OnGUI();
        }

        // ReSharper disable once UnusedParameter.Local
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            Plugin.Log.LogError(
                $"Error patching MonoBehaviour.OnGUI in all types, closing tool since continuing can cause desyncs: {ex}");
            return ex;
        }
    }
}