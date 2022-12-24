using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.MonoBehaviourController;
using UnityEngine;

// ReSharper disable UnusedMember.Local

namespace UniTASPlugin.LegacyPatches.UnityEngine;

[HarmonyPatch]
public class MonoBehaviourPatch
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

    private static PluginWrapper pluginWrapper;
    private static PluginWrapper PluginWrapper => pluginWrapper ??= Plugin.Kernel.GetInstance<PluginWrapper>();

    private static IMonoBehaviourController monoBehaviourController;

    private static IMonoBehaviourController MonoBehaviourController =>
        monoBehaviourController ??= Plugin.Kernel.GetInstance<IMonoBehaviourController>();

    [HarmonyPatch]
    private class AwakeMultiple
    {
        public static IEnumerable<MethodBase> TargetMethods() => GetEventMethods("Awake");

        public static void Prefix()
        {
            PluginWrapper.Awake();
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
            PluginWrapper.PreFixedUpdate();
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
            PluginWrapper.Update();
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
            PluginWrapper.OnEnable();
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
            PluginWrapper.Start();
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

    // used for other events
    [HarmonyPatch]
    private class MonoBehaviourExecutionController
    {
        private static readonly string[] EventMethods =
        {
            "Awake",
            "FixedUpdate",
            "LateUpdate",
            "OnAnimatorIK",
            "OnAnimatorMove",
            "OnApplicationFocus",
            "OnApplicationPause",
            "OnApplicationQuit",
            "OnAudioFilterRead",
            "OnBecameInvisible",
            "OnBecameVisible",
            "OnCollisionEnter",
            "OnCollisionEnter2D",
            "OnCollisionExit",
            "OnCollisionExit2D",
            "OnCollisionStay",
            "OnCollisionStay2D",
            "OnConnectedToServer",
            "OnControllerColliderHit",
            "OnDestroy",
            "OnDisable",
            "OnDisconnectedFromServer",
            "OnDrawGizmos",
            "OnDrawGizmosSelected",
            "OnEnable",
            "OnFailedToConnect",
            "OnFailedToConnectToMasterServer",
            "OnJointBreak",
            "OnJointBreak2D",
            "OnMasterServerEvent",
            "OnMouseDown",
            "OnMouseDrag",
            "OnMouseEnter",
            "OnMouseExit",
            "OnMouseOver",
            "OnMouseUp",
            "OnMouseUpAsButton",
            "OnNetworkInstantiate",
            "OnParticleCollision",
            "OnParticleSystemStopped",
            "OnParticleTrigger",
            "OnParticleUpdateJobScheduled",
            "OnPlayerConnected",
            "OnPlayerDisconnected",
            "OnPostRender",
            "OnPreCull",
            "OnPreRender",
            "OnRenderImage",
            "OnRenderObject",
            "OnSerializeNetworkView",
            "OnServerInitialized",
            "OnTransformChildrenChanged",
            "OnTransformParentChanged",
            "OnTriggerEnter",
            "OnTriggerEnter2D",
            "OnTriggerExit",
            "OnTriggerExit2D",
            "OnTriggerStay",
            "OnTriggerStay2D",
            "OnValidate",
            "OnWillRenderObject",
            "Reset",
            "Start",
            "Update",
            "OnGUI"
        };

        private static readonly string[] ExcludeNamespaces =
        {
            "TMPro",
            "UnityEngine",
            "Unity",
            "UniTASPlugin",
            "BepInEx"
        };

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
                        // TODO remove hardcoded type name
                        if (!type.IsAbstract && type.IsSubclassOf(typeof(MonoBehaviour)) && (type.FullName == null ||
                                !ExcludeNamespaces.Any(type.FullName.StartsWith)))
                        {
                            Plugin.Log.LogDebug($"type name: {type.FullName}");
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
                foreach (var eventMethod in EventMethods)
                {
                    var foundMethod = monoBehaviourType.GetMethod(eventMethod,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes,
                        null);
                    if (foundMethod != null)
                        yield return foundMethod;
                }
            }
        }

        public static bool Prefix()
        {
            return !MonoBehaviourController.PausedExecution;
        }

        // ReSharper disable once UnusedParameter.Local
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            if (ex != null)
            {
                Plugin.Log.LogFatal(
                    $"Error patching MonoBehaviour event methods in all types, closing tool since continuing can cause desyncs: {ex}");
            }

            return ex;
        }
    }
}