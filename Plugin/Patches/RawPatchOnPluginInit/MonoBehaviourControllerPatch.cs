using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.MonoBehaviourController;
using UnityEngine;

namespace UniTASPlugin.Patches.RawPatchOnPluginInit;

[PatchTypes.RawPatchOnPluginInit]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class MonoBehaviourControllerPatch
{
    private static readonly IMonoBehaviourController MonoBehaviourController =
        Plugin.Kernel.GetInstance<IMonoBehaviourController>();

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
                                !ExcludeNamespaces.Any(x => type.Namespace != null && type.Namespace.StartsWith(x))))
                        {
                            monoBehaviourTypes.Add(type);
                            Trace.Write($"Target MonoBehavior pause patch type: {type.FullName}");
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