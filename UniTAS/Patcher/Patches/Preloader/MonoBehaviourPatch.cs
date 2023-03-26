using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.Shared;

namespace UniTAS.Patcher.Patches.Preloader;

public class MonoBehaviourPatch : PreloadPatcher
{
    public override IEnumerable<string> TargetDLLs => Utils.AllTargetDllsWithGenericExclusions;

    private const string COLLISION = "UnityEngine.Collision";
    private const string COLLISION_2D = "UnityEngine.Collision2D";
    private const string CONTROLLER_COLLIDER_HIT = "UnityEngine.ControllerColliderHit";
    private const string NETWORK_DISCONNECTION = "UnityEngine.NetworkDisconnection";
    private const string NETWORK_CONNECTION_ERROR = "UnityEngine.NetworkConnectionError";
    private const string NETWORK_MESSAGE_INFO = "UnityEngine.NetworkMessageInfo";
    private const string NETWORK_PLAYER = "UnityEngine.NetworkPlayer";
    private const string MASTER_SERVER_EVENT = "UnityEngine.MasterServerEvent";
    private const string COLLIDER = "UnityEngine.Collider";
    private const string COLLIDER_2D = "UnityEngine.Collider2D";
    private const string GAME_OBJECT = "UnityEngine.GameObject";
    private const string RENDER_TEXTURE = "UnityEngine.RenderTexture";
    private const string BIT_STREAM = "UnityEngine.BitStream";

    private static readonly KeyValuePair<string, MethodBase>[] EventMethods =
    {
        new("Awake", AccessTools.Method(typeof(MonoBehaviourEvents), nameof(MonoBehaviourEvents.InvokeAwake))),
        new("OnEnable", AccessTools.Method(typeof(MonoBehaviourEvents), nameof(MonoBehaviourEvents.InvokeOnEnable))),
        new("Start", AccessTools.Method(typeof(MonoBehaviourEvents), nameof(MonoBehaviourEvents.InvokeStart))),
        new("Update", AccessTools.Method(typeof(MonoBehaviourEvents), nameof(MonoBehaviourEvents.InvokeUpdate))),
        new("LateUpdate",
            AccessTools.Method(typeof(MonoBehaviourEvents), nameof(MonoBehaviourEvents.InvokeLateUpdate))),
        new("FixedUpdate",
            AccessTools.Method(typeof(MonoBehaviourEvents), nameof(MonoBehaviourEvents.InvokeFixedUpdate)))
        // new("OnGUI", AccessTools.Method(typeof(MonoBehaviourEvents), nameof(MonoBehaviourEvents.InvokeOnGUI)))
    };

    // event methods, with list of arg types
    // arg types in mono beh are always positional, so we can use this to determine which method to call
    // args are optionally available in the event method
    private static readonly KeyValuePair<string, string[]>[] PauseEventMethods =
    {
        new("Awake", new string[0]),
        new("FixedUpdate", new string[0]),
        new("LateUpdate", new string[0]),
        new("OnAnimatorIK", new[] { typeof(int).FullName }),
        new("OnAnimatorMove", new string[0]),
        new("OnApplicationFocus", new[] { typeof(bool).FullName }),
        new("OnApplicationPause", new[] { typeof(bool).FullName }),
        new("OnApplicationQuit", new string[0]),
        new("OnAudioFilterRead", new[] { typeof(float[]).FullName, typeof(int).FullName }),
        new("OnBecameInvisible", new string[0]),
        new("OnBecameVisible", new string[0]),
        new("OnCollisionEnter", new[] { COLLISION }),
        new("OnCollisionEnter2D", new[] { COLLISION_2D }),
        new("OnCollisionExit", new[] { COLLISION }),
        new("OnCollisionExit2D", new[] { COLLISION_2D }),
        new("OnCollisionStay", new[] { COLLISION }),
        new("OnCollisionStay2D", new[] { COLLISION_2D }),
        new("OnConnectedToServer", new string[0]),
        new("OnControllerColliderHit", new[] { CONTROLLER_COLLIDER_HIT }),
        new("OnDestroy", new string[0]),
        new("OnDisable", new string[0]),
        new("OnDisconnectedFromServer", new[] { NETWORK_DISCONNECTION }),
        new("OnDrawGizmos", new string[0]),
        new("OnDrawGizmosSelected", new string[0]),
        new("OnEnable", new string[0]),
        new("OnFailedToConnect", new[] { NETWORK_CONNECTION_ERROR }),
        new("OnFailedToConnectToMasterServer", new[] { NETWORK_CONNECTION_ERROR }),
        new("OnJointBreak", new[] { typeof(float).FullName }),
        new("OnJointBreak2D", new[] { typeof(float).FullName }),
        new("OnMasterServerEvent", new[] { MASTER_SERVER_EVENT }),
        new("OnMouseDown", new string[0]),
        new("OnMouseDrag", new string[0]),
        new("OnMouseEnter", new string[0]),
        new("OnMouseExit", new string[0]),
        new("OnMouseOver", new string[0]),
        new("OnMouseUp", new string[0]),
        new("OnMouseUpAsButton", new string[0]),
        new("OnNetworkInstantiate", new[] { NETWORK_MESSAGE_INFO }),
        new("OnParticleCollision", new[] { GAME_OBJECT }),
        new("OnParticleSystemStopped", new string[0]),
        new("OnParticleTrigger", new string[0]),
        new("OnParticleUpdateJobScheduled", new string[0]),
        new("OnPlayerConnected", new[] { NETWORK_PLAYER }),
        new("OnPlayerDisconnected", new[] { NETWORK_PLAYER }),
        new("OnPostRender", new string[0]),
        new("OnPreCull", new string[0]),
        new("OnPreRender", new string[0]),
        new("OnRenderImage", new[] { RENDER_TEXTURE, RENDER_TEXTURE }),
        new("OnRenderObject", new string[0]),
        new("OnSerializeNetworkView", new[] { BIT_STREAM, NETWORK_MESSAGE_INFO }),
        new("OnServerInitialized", new string[0]),
        new("OnTransformChildrenChanged", new string[0]),
        new("OnTransformParentChanged", new string[0]),
        new("OnTriggerEnter", new[] { COLLIDER }),
        new("OnTriggerEnter2D", new[] { COLLIDER_2D }),
        new("OnTriggerExit", new[] { COLLIDER }),
        new("OnTriggerExit2D", new[] { COLLIDER_2D }),
        new("OnTriggerStay", new[] { COLLIDER }),
        new("OnTriggerStay2D", new[] { COLLIDER_2D }),
        new("OnValidate", new string[0]),
        new("OnWillRenderObject", new string[0]),
        new("Reset", new string[0]),
        new("Start", new string[0]),
        new("Update", new string[0]),
        new("OnGUI", new string[0])
    };

    private static readonly string[] ExcludeNamespaces =
    {
        // TODO remove this exclusion, i dont know why i added it
        "TMPro",
        "UnityEngine",
        "Unity",
        "UniTAS.Plugin",
        "UniTAS.Patcher",
        "BepInEx"
    };

    public override void Patch(ref AssemblyDefinition assembly)
    {
        var types = assembly.Modules.SelectMany(m => m.GetAllTypes());
        var pauseExecutionProperty = AccessTools.Property(typeof(MonoBehaviourController),
            nameof(MonoBehaviourController.PausedExecution)).GetGetMethod();
        var pauseExecutionReference = assembly.MainModule.ImportReference(pauseExecutionProperty);

        foreach (var type in types)
        {
            // check if type base is MonoBehaviour
            var isMonoBehaviour = false;
            var baseType = type.BaseType?.Resolve();
            while (baseType != null)
            {
                if (baseType.FullName == "UnityEngine.MonoBehaviour")
                {
                    isMonoBehaviour = true;
                    break;
                }

                baseType = baseType.BaseType?.Resolve();
            }

            if (!isMonoBehaviour) continue;

            // method invoke pause
            if (!ExcludeNamespaces.Any(type.Namespace.StartsWith))
            {
                foreach (var eventMethodPair in PauseEventMethods)
                {
                    var foundMethod =
                        type.Methods.FirstOrDefault(m => m.Name == eventMethodPair.Key && !m.HasParameters);

                    if (foundMethod == null)
                    {
                        for (var i = 0; i < eventMethodPair.Value.Length; i++)
                        {
                            var parameterTypes = eventMethodPair.Value.Take(i + 1).ToArray();
                            foundMethod = type.Methods.FirstOrDefault(m =>
                                m.Name == eventMethodPair.Key && m.HasParameters &&
                                m.Parameters.Select(x => x.ParameterType.FullName).SequenceEqual(parameterTypes));

                            if (foundMethod != null) break;
                        }
                    }

                    if (foundMethod == null) continue;

                    Trace.Write($"Patching method for pausing execution {foundMethod.FullName}");

                    var il = foundMethod.Body.GetILProcessor();
                    var firstInstruction = il.Body.Instructions.First();

                    // return early check
                    il.InsertBefore(firstInstruction, il.Create(OpCodes.Call, pauseExecutionReference));
                    il.InsertBefore(firstInstruction, il.Create(OpCodes.Brfalse_S, firstInstruction));
                    il.InsertBefore(firstInstruction, il.Create(OpCodes.Ret));
                }
            }

            // event methods invoke
            foreach (var eventMethodPair in EventMethods)
            {
                InvokeUnityEventMethod(type, eventMethodPair.Key, assembly, eventMethodPair.Value);
            }
        }
    }

    private static void InvokeUnityEventMethod(TypeDefinition type, string methodName, AssemblyDefinition assembly,
        MethodBase eventInvoker)
    {
        var method = type.Methods.FirstOrDefault(m => !m.IsStatic && m.Name == methodName && !m.HasParameters);
        if (method == null) return;

        var ilProcessor = method.Body.GetILProcessor();
        var reference = assembly.MainModule.ImportReference(eventInvoker);

        ilProcessor.InsertBefore(method.Body.Instructions.First(), ilProcessor.Create(OpCodes.Call, reference));

        Trace.Write($"Successfully patched {methodName} for type {type.FullName} for updates");
    }
}