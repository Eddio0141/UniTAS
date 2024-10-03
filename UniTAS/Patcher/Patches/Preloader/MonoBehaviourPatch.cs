using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UniTAS.Patcher.ContainerBindings.GameExecutionControllers;
using UniTAS.Patcher.ContainerBindings.UnityEvents;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Preloader;

public class MonoBehaviourPatch : PreloadPatcher
{
    public override IEnumerable<string> TargetDLLs => TargetPatcherDlls.AllExcludedDLLs;

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

    private static readonly (string, MethodBase)[] EventMethods =
    [
        new("Awake",
            AccessTools.Method(typeof(UnityEventInvokers), nameof(UnityEventInvokers.InvokeAwake))),
        new("OnEnable",
            AccessTools.Method(typeof(UnityEventInvokers), nameof(UnityEventInvokers.InvokeOnEnable))),
        new("Start",
            AccessTools.Method(typeof(UnityEventInvokers), nameof(UnityEventInvokers.InvokeStart))),
        new("Update",
            AccessTools.Method(typeof(UnityEventInvokers), nameof(UnityEventInvokers.InvokeUpdate))),
        new("LateUpdate",
            AccessTools.Method(typeof(UnityEventInvokers), nameof(UnityEventInvokers.InvokeLateUpdate))),
        new("FixedUpdate",
            AccessTools.Method(typeof(UnityEventInvokers), nameof(UnityEventInvokers.InvokeFixedUpdate)))
        // new("OnGUI", AccessTools.Method(typeof(MonoBehaviourEvents), nameof(MonoBehaviourEvents.InvokeOnGUI)))
    ];

    // event methods, with list of arg types
    // arg types in mono beh are always positional, so we can use this to determine which method to call
    // args are optionally available in the event method
    private static readonly (string, string[])[] PauseEventMethods =
    [
        new("Awake", []),
        new("FixedUpdate", []),
        new("LateUpdate", []),
        new("OnAnimatorIK", [typeof(int).FullName]),
        new("OnAnimatorMove", []),
        new("OnApplicationFocus", [typeof(bool).FullName]),
        new("OnApplicationPause", [typeof(bool).FullName]),
        new("OnApplicationQuit", []),
        new("OnAudioFilterRead", [typeof(float[]).FullName, typeof(int).FullName]),
        new("OnBecameInvisible", []),
        new("OnBecameVisible", []),
        new("OnCollisionEnter", [COLLISION]),
        new("OnCollisionEnter2D", [COLLISION_2D]),
        new("OnCollisionExit", [COLLISION]),
        new("OnCollisionExit2D", [COLLISION_2D]),
        new("OnCollisionStay", [COLLISION]),
        new("OnCollisionStay2D", [COLLISION_2D]),
        new("OnConnectedToServer", []),
        new("OnControllerColliderHit", [CONTROLLER_COLLIDER_HIT]),
        new("OnDestroy", []),
        new("OnDisable", []),
        new("OnDisconnectedFromServer", [NETWORK_DISCONNECTION]),
        new("OnDrawGizmos", []),
        new("OnDrawGizmosSelected", []),
        new("OnEnable", []),
        new("OnFailedToConnect", [NETWORK_CONNECTION_ERROR]),
        new("OnFailedToConnectToMasterServer", [NETWORK_CONNECTION_ERROR]),
        new("OnJointBreak", [typeof(float).FullName]),
        new("OnJointBreak2D", [typeof(float).FullName]),
        new("OnMasterServerEvent", [MASTER_SERVER_EVENT]),
        new("OnMouseDown", []),
        new("OnMouseDrag", []),
        new("OnMouseEnter", []),
        new("OnMouseExit", []),
        new("OnMouseOver", []),
        new("OnMouseUp", []),
        new("OnMouseUpAsButton", []),
        new("OnNetworkInstantiate", [NETWORK_MESSAGE_INFO]),
        new("OnParticleCollision", [GAME_OBJECT]),
        new("OnParticleSystemStopped", []),
        new("OnParticleTrigger", []),
        new("OnParticleUpdateJobScheduled", []),
        new("OnPlayerConnected", [NETWORK_PLAYER]),
        new("OnPlayerDisconnected", [NETWORK_PLAYER]),
        new("OnPostRender", []),
        new("OnPreCull", []),
        new("OnPreRender", []),
        new("OnRenderImage", [RENDER_TEXTURE, RENDER_TEXTURE]),
        new("OnRenderObject", []),
        new("OnSerializeNetworkView", [BIT_STREAM, NETWORK_MESSAGE_INFO]),
        new("OnServerInitialized", []),
        new("OnTransformChildrenChanged", []),
        new("OnTransformParentChanged", []),
        new("OnTriggerEnter", [COLLIDER]),
        new("OnTriggerEnter2D", [COLLIDER_2D]),
        new("OnTriggerExit", [COLLIDER]),
        new("OnTriggerExit2D", [COLLIDER_2D]),
        new("OnTriggerStay", [COLLIDER]),
        new("OnTriggerStay2D", [COLLIDER_2D]),
        new("OnValidate", []),
        new("OnWillRenderObject", []),
        new("Reset", []),
        new("Start", []),
        new("Update", []),
        new("OnGUI", [])
    ];

    public override void Patch(ref AssemblyDefinition assembly)
    {
        var types = assembly.Modules.SelectMany(m => m.GetAllTypes());
        var pauseExecutionProperty = AccessTools.Property(typeof(MonoBehaviourController),
            nameof(MonoBehaviourController.PausedExecution)).GetGetMethod();
        var pauseExecutionReference = assembly.MainModule.ImportReference(pauseExecutionProperty);
        var pausedUpdateProperty = AccessTools.Property(typeof(MonoBehaviourController),
            nameof(MonoBehaviourController.PausedUpdate)).GetGetMethod();
        var pausedUpdateReference = assembly.MainModule.ImportReference(pausedUpdateProperty);

        foreach (var type in types)
        {
            if (!type.IsMonoBehaviour()) continue;

            StaticLogger.Log.LogDebug($"Patching MonoBehaviour type: {type.FullName}");

            // method invoke pause
            foreach (var eventMethodPair in PauseEventMethods)
            {
                var (eventMethodName, eventMethodArgs) = eventMethodPair;

                // try finding method with no parameters
                var eventMethodsMatch = type.GetMethods().Where(x => x.Name == eventMethodName).ToList();
                var foundMethod =
                    eventMethodsMatch.FirstOrDefault(m => !m.HasParameters);

                // ok try finding method with parameters one by one
                // it doesn't matter if the method only has part of the parameters, it just matters it comes in the right order
                if (foundMethod == null)
                {
                    for (var i = 0; i < eventMethodArgs.Length; i++)
                    {
                        var parameterTypes = eventMethodArgs.Take(i + 1).ToArray();
                        foundMethod = eventMethodsMatch.FirstOrDefault(m =>
                            m.HasParameters && m.Parameters.Select(x => x.ParameterType.FullName)
                                .SequenceEqual(parameterTypes));

                        if (foundMethod != null) break;
                    }
                }

                if (foundMethod is not { HasBody: true }) continue;

                StaticLogger.Trace($"Patching method for pausing execution {foundMethod.FullName}");

                foundMethod.Body.SimplifyMacros();
                var il = foundMethod.Body.GetILProcessor();
                var firstInstruction = il.Body.Instructions.First();

                // return early check
                il.InsertBeforeInstructionReplace(firstInstruction, il.Create(OpCodes.Call, pauseExecutionReference),
                    InstructionReplaceFixType.ExceptionRanges);
                il.InsertBeforeInstructionReplace(firstInstruction, il.Create(OpCodes.Brfalse_S, firstInstruction),
                    InstructionReplaceFixType.ExceptionRanges);

                // if the return type isn't void, we need to return a default value
                if (foundMethod.ReturnType != assembly.MainModule.TypeSystem.Void)
                {
                    // if value type, we need to return a default value
                    if (foundMethod.ReturnType.IsValueType)
                    {
                        var local = new VariableDefinition(foundMethod.ReturnType);
                        il.Body.Variables.Add(local);
                        il.InsertBeforeInstructionReplace(firstInstruction, il.Create(OpCodes.Ldloca_S, local),
                            InstructionReplaceFixType.ExceptionRanges);
                        il.InsertBeforeInstructionReplace(firstInstruction,
                            il.Create(OpCodes.Initobj, foundMethod.ReturnType),
                            InstructionReplaceFixType.ExceptionRanges);
                        il.InsertBeforeInstructionReplace(firstInstruction, il.Create(OpCodes.Ldloc_S, local),
                            InstructionReplaceFixType.ExceptionRanges);
                    }
                    else
                    {
                        il.InsertBeforeInstructionReplace(firstInstruction, il.Create(OpCodes.Ldnull),
                            InstructionReplaceFixType.ExceptionRanges);
                    }
                }

                il.InsertBeforeInstructionReplace(firstInstruction, il.Create(OpCodes.Ret),
                    InstructionReplaceFixType.ExceptionRanges);

                foundMethod.Body.OptimizeMacros();
            }

            // update skip check and related methods
            var updateMethods = new[]
            {
                type.Methods.FirstOrDefault(m => m.Name == "Update" && !m.HasParameters),
                type.Methods.FirstOrDefault(m => m.Name == "LateUpdate" && !m.HasParameters)
            };

            foreach (var updateMethod in updateMethods)
            {
                UpdateEarlyReturn(updateMethod, assembly, pausedUpdateReference);
            }

            StaticLogger.Trace("Patched Update related methods for skipping execution");

            // event methods invoke
            foreach (var eventMethodPair in EventMethods)
            {
                InvokeUnityEventMethod(type, eventMethodPair.Item1, assembly, eventMethodPair.Item2);
            }
        }
    }

    private static void UpdateEarlyReturn(MethodDefinition skipCheckMethod, AssemblyDefinition assembly,
        MethodReference pausedUpdateReference)
    {
        if (skipCheckMethod is not { HasBody: true }) return;
        skipCheckMethod.Body.SimplifyMacros();
        var updateIl = skipCheckMethod.Body.GetILProcessor();
        var updateFirstInstruction = updateIl.Body.Instructions.First();

        // return early check
        updateIl.InsertBeforeInstructionReplace(updateFirstInstruction,
            updateIl.Create(OpCodes.Call, pausedUpdateReference), InstructionReplaceFixType.ExceptionRanges);
        updateIl.InsertBeforeInstructionReplace(updateFirstInstruction,
            updateIl.Create(OpCodes.Brfalse_S, updateFirstInstruction), InstructionReplaceFixType.ExceptionRanges);

        // if the return type isn't void, we need to return a default value
        var skipCheckMethodReturn = skipCheckMethod.ReturnType;
        if (skipCheckMethodReturn != assembly.MainModule.TypeSystem.Void)
        {
            // if value type, we need to return a default value
            if (skipCheckMethodReturn.IsValueType)
            {
                var local = new VariableDefinition(skipCheckMethodReturn);
                updateIl.Body.Variables.Add(local);
                updateIl.InsertBeforeInstructionReplace(updateFirstInstruction,
                    updateIl.Create(OpCodes.Ldloca_S, local),
                    InstructionReplaceFixType.ExceptionRanges);
                updateIl.InsertBeforeInstructionReplace(updateFirstInstruction,
                    updateIl.Create(OpCodes.Initobj, skipCheckMethodReturn),
                    InstructionReplaceFixType.ExceptionRanges);
                updateIl.InsertBeforeInstructionReplace(updateFirstInstruction, updateIl.Create(OpCodes.Ldloc_S, local),
                    InstructionReplaceFixType.ExceptionRanges);
            }
            else
            {
                updateIl.InsertBeforeInstructionReplace(updateFirstInstruction, updateIl.Create(OpCodes.Ldnull),
                    InstructionReplaceFixType.ExceptionRanges);
            }
        }

        updateIl.InsertBeforeInstructionReplace(updateFirstInstruction, updateIl.Create(OpCodes.Ret),
            InstructionReplaceFixType.ExceptionRanges);

        skipCheckMethod.Body.OptimizeMacros();
    }

    private static void InvokeUnityEventMethod(TypeDefinition type, string methodName, AssemblyDefinition assembly,
        MethodBase eventInvoker)
    {
        var method = type.Methods.FirstOrDefault(m => !m.IsStatic && m.Name == methodName && !m.HasParameters);
        if (method is not { HasBody: true }) return;

        method.Body.SimplifyMacros();
        var ilProcessor = method.Body.GetILProcessor();
        var reference = assembly.MainModule.ImportReference(eventInvoker);

        // shouldn't be referenced by anything as it gets invoked once per type
        ilProcessor.InsertBeforeInstructionReplace(method.Body.Instructions.First(),
            ilProcessor.Create(OpCodes.Call, reference), InstructionReplaceFixType.ExceptionRanges);

        method.Body.OptimizeMacros();

        StaticLogger.Trace(
            $"Successfully patched {methodName} for type {type.FullName} for updates, invokes {eventInvoker.Name}");
    }
}