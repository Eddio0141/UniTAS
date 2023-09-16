using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Utils;
using UniTAS.Patcher.ContainerBindings.GameExecutionControllers;
using UniTAS.Patcher.ContainerBindings.UnityEvents;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Preloader;

public class MonoBehaviourPatch : PreloadPatcher
{
    public override IEnumerable<string> TargetDLLs => TargetPatcherDlls.AllDLLs.Where(x =>
    {
        var fileWithoutExtension = Path.GetFileNameWithoutExtension(x);
        return fileWithoutExtension == null ||
               StaticCtorPatchTargetInfo.AssemblyIncludeRaw.Any(a => fileWithoutExtension.Like(a)) ||
               !StaticCtorPatchTargetInfo.AssemblyExclusionsRaw.Any(a => fileWithoutExtension.Like(a));
    });

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
    {
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
    };

    // event methods, with list of arg types
    // arg types in mono beh are always positional, so we can use this to determine which method to call
    // args are optionally available in the event method
    private static readonly (string, string[])[] PauseEventMethods =
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
            // check if type base is MonoBehaviour
            var isMonoBehaviour = false;
            var baseType = type.BaseType?.SafeResolve();
            while (baseType != null)
            {
                if (baseType.FullName == "UnityEngine.MonoBehaviour")
                {
                    isMonoBehaviour = true;
                    break;
                }

                baseType = baseType.BaseType?.SafeResolve();
            }

            if (!isMonoBehaviour) continue;

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

                StaticLogger.Log.LogDebug($"Patching method for pausing execution {foundMethod.FullName}");

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

            StaticLogger.Log.LogDebug("Patched Update related methods for skipping execution");

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

        StaticLogger.Log.LogDebug(
            $"Successfully patched {methodName} for type {type.FullName} for updates, invokes {eventInvoker.Name}");
    }
}