using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.MonoBehaviourController;
using UnityEngine;

namespace UniTAS.Plugin.Patches.RawPatchOnPluginInit;

[PatchTypes.RawPatchOnPluginInit]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class MonoBehaviourControllerPatch
{
    private static readonly IMonoBehaviourController MonoBehaviourController =
        Plugin.Kernel.GetInstance<IMonoBehaviourController>();

    private static readonly Type Collision = AccessTools.TypeByName("UnityEngine.Collision");
    private static readonly Type Collision2D = AccessTools.TypeByName("UnityEngine.Collision2D");
    private static readonly Type ControllerColliderHit = AccessTools.TypeByName("UnityEngine.ControllerColliderHit");
    private static readonly Type NetworkDisconnection = AccessTools.TypeByName("UnityEngine.NetworkDisconnection");
    private static readonly Type NetworkConnectionError = AccessTools.TypeByName("UnityEngine.NetworkConnectionError");
    private static readonly Type NetworkMessageInfo = AccessTools.TypeByName("UnityEngine.NetworkMessageInfo");
    private static readonly Type NetworkPlayer = AccessTools.TypeByName("UnityEngine.NetworkPlayer");
    private static readonly Type MasterServerEvent = AccessTools.TypeByName("UnityEngine.MasterServerEvent");
    private static readonly Type Collider = AccessTools.TypeByName("UnityEngine.Collider");
    private static readonly Type Collider2D = AccessTools.TypeByName("UnityEngine.Collider2D");
    private static readonly Type GameObject = AccessTools.TypeByName("UnityEngine.GameObject");
    private static readonly Type RenderTexture = AccessTools.TypeByName("UnityEngine.RenderTexture");
    private static readonly Type BitStream = AccessTools.TypeByName("UnityEngine.BitStream");

    // used for other events
    [HarmonyPatch]
    private class MonoBehaviourExecutionController
    {
        // event methods, with list of arg types
        // arg types in mono beh are always positional, so we can use this to determine which method to call
        // args are optionally available in the event method
        private static readonly KeyValuePair<string, Type[]>[] EventMethods =
        {
            new("Awake", Type.EmptyTypes),
            new("FixedUpdate", Type.EmptyTypes),
            new("LateUpdate", Type.EmptyTypes),
            new("OnAnimatorIK", new[] { typeof(int) }),
            new("OnAnimatorMove", Type.EmptyTypes),
            new("OnApplicationFocus", new[] { typeof(bool) }),
            new("OnApplicationPause", new[] { typeof(bool) }),
            new("OnApplicationQuit", Type.EmptyTypes),
            new("OnAudioFilterRead", new[] { typeof(float[]), typeof(int) }),
            new("OnBecameInvisible", Type.EmptyTypes),
            new("OnBecameVisible", Type.EmptyTypes),
            new("OnCollisionEnter", new[] { Collision }),
            new("OnCollisionEnter2D", new[] { Collision2D }),
            new("OnCollisionExit", new[] { Collision }),
            new("OnCollisionExit2D", new[] { Collision2D }),
            new("OnCollisionStay", new[] { Collision }),
            new("OnCollisionStay2D", new[] { Collision2D }),
            new("OnConnectedToServer", Type.EmptyTypes),
            new("OnControllerColliderHit", new[] { ControllerColliderHit }),
            new("OnDestroy", Type.EmptyTypes),
            new("OnDisable", Type.EmptyTypes),
            new("OnDisconnectedFromServer", new[] { NetworkDisconnection }),
            new("OnDrawGizmos", Type.EmptyTypes),
            new("OnDrawGizmosSelected", Type.EmptyTypes),
            new("OnEnable", Type.EmptyTypes),
            new("OnFailedToConnect", new[] { NetworkConnectionError }),
            new("OnFailedToConnectToMasterServer", new[] { NetworkConnectionError }),
            new("OnJointBreak", new[] { typeof(float) }),
            new("OnJointBreak2D", new[] { typeof(float) }),
            new("OnMasterServerEvent", new[] { MasterServerEvent }),
            new("OnMouseDown", Type.EmptyTypes),
            new("OnMouseDrag", Type.EmptyTypes),
            new("OnMouseEnter", Type.EmptyTypes),
            new("OnMouseExit", Type.EmptyTypes),
            new("OnMouseOver", Type.EmptyTypes),
            new("OnMouseUp", Type.EmptyTypes),
            new("OnMouseUpAsButton", Type.EmptyTypes),
            new("OnNetworkInstantiate", new[] { NetworkMessageInfo }),
            new("OnParticleCollision", new[] { GameObject }),
            new("OnParticleSystemStopped", Type.EmptyTypes),
            new("OnParticleTrigger", Type.EmptyTypes),
            new("OnParticleUpdateJobScheduled", Type.EmptyTypes),
            new("OnPlayerConnected", new[] { NetworkPlayer }),
            new("OnPlayerDisconnected", new[] { NetworkPlayer }),
            new("OnPostRender", Type.EmptyTypes),
            new("OnPreCull", Type.EmptyTypes),
            new("OnPreRender", Type.EmptyTypes),
            new("OnRenderImage", new[] { RenderTexture, RenderTexture }),
            new("OnRenderObject", Type.EmptyTypes),
            new("OnSerializeNetworkView", new[] { BitStream, NetworkMessageInfo }),
            new("OnServerInitialized", Type.EmptyTypes),
            new("OnTransformChildrenChanged", Type.EmptyTypes),
            new("OnTransformParentChanged", Type.EmptyTypes),
            new("OnTriggerEnter", new[] { Collider }),
            new("OnTriggerEnter2D", new[] { Collider2D }),
            new("OnTriggerExit", new[] { Collider }),
            new("OnTriggerExit2D", new[] { Collider2D }),
            new("OnTriggerStay", new[] { Collider }),
            new("OnTriggerStay2D", new[] { Collider2D }),
            new("OnValidate", Type.EmptyTypes),
            new("OnWillRenderObject", Type.EmptyTypes),
            new("Reset", Type.EmptyTypes),
            new("Start", Type.EmptyTypes),
            new("Update", Type.EmptyTypes),
            new("OnGUI", Type.EmptyTypes)
        };

        private static readonly string[] ExcludeNamespaces =
        {
            // TODO remove this exclusion, i dont know why i added it
            "TMPro",
            "UnityEngine",
            "Unity",
            typeof(Plugin).Namespace,
            "BepInEx"
        };

        public static IEnumerable<MethodBase> TargetMethods()
        {
            var monoBehaviourTypes = new List<Type>();
            foreach (var assembly in AccessTools.AllAssemblies())
            {
                var types = AccessTools.GetTypesFromAssembly(assembly);
                foreach (var type in types)
                {
                    // TODO remove hardcoded MonoBehavior referencing
                    if (!type.IsAbstract && type.IsSubclassOf(typeof(MonoBehaviour)) && (type.FullName == null ||
                            !ExcludeNamespaces.Any(x => type.Namespace != null && type.Namespace.StartsWith(x))))
                    {
                        monoBehaviourTypes.Add(type);
                        Trace.Write($"Target MonoBehavior pause patch type: {type.FullName}");
                    }
                }
            }

            foreach (var monoBehaviourType in monoBehaviourTypes)
            {
                foreach (var eventMethod in EventMethods)
                {
                    var foundMethod = monoBehaviourType.GetMethod(eventMethod.Key,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes,
                        null);

                    if (foundMethod == null)
                    {
                        for (var i = 0; i < eventMethod.Value.Length; i++)
                        {
                            if (eventMethod.Value[i] == null) break;
                            var parameterTypes = eventMethod.Value.Take(i + 1).ToArray();
                            foundMethod = monoBehaviourType.GetMethod(eventMethod.Key,
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                                parameterTypes, null);

                            if (foundMethod != null) break;
                        }
                    }

                    if (foundMethod == null) continue;

                    Trace.Write(
                        $"Target MonoBehavior pause patch method: {monoBehaviourType.FullName}.{eventMethod.Key}");
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
                Plugin.Log.LogWarning(
                    $"Error patching MonoBehaviour event method, Method: {original.DeclaringType?.FullName ?? "unknown_type"}.{original.Name}, Exception: {ex}");
            }

            return null;
        }
    }
}