using System.Collections;
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
using UniTAS.Patcher.ManualServices;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Preloader;

public class MonoBehaviourPatch : PreloadPatcher
{
    private const string Collision = "UnityEngine.Collision";
    private const string Collision2D = "UnityEngine.Collision2D";
    private const string ControllerColliderHit = "UnityEngine.ControllerColliderHit";
    private const string NetworkDisconnection = "UnityEngine.NetworkDisconnection";
    private const string NetworkConnectionError = "UnityEngine.NetworkConnectionError";
    private const string NetworkMessageInfo = "UnityEngine.NetworkMessageInfo";
    private const string NetworkPlayer = "UnityEngine.NetworkPlayer";
    private const string MasterServerEvent = "UnityEngine.MasterServerEvent";
    private const string Collider = "UnityEngine.Collider";
    private const string Collider2D = "UnityEngine.Collider2D";
    private const string GameObject = "UnityEngine.GameObject";
    private const string RenderTexture = "UnityEngine.RenderTexture";
    private const string BITStream = "UnityEngine.BitStream";

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
        new("OnCollisionEnter", [Collision]),
        new("OnCollisionEnter2D", [Collision2D]),
        new("OnCollisionExit", [Collision]),
        new("OnCollisionExit2D", [Collision2D]),
        new("OnCollisionStay", [Collision]),
        new("OnCollisionStay2D", [Collision2D]),
        new("OnConnectedToServer", []),
        new("OnControllerColliderHit", [ControllerColliderHit]),
        new("OnDestroy", []),
        new("OnDisable", []),
        new("OnDisconnectedFromServer", [NetworkDisconnection]),
        new("OnDrawGizmos", []),
        new("OnDrawGizmosSelected", []),
        new("OnEnable", []),
        new("OnFailedToConnect", [NetworkConnectionError]),
        new("OnFailedToConnectToMasterServer", [NetworkConnectionError]),
        new("OnJointBreak", [typeof(float).FullName]),
        new("OnJointBreak2D", [typeof(float).FullName]),
        new("OnMasterServerEvent", [MasterServerEvent]),
        new("OnMouseDown", []),
        new("OnMouseDrag", []),
        new("OnMouseEnter", []),
        new("OnMouseExit", []),
        new("OnMouseOver", []),
        new("OnMouseUp", []),
        new("OnMouseUpAsButton", []),
        new("OnNetworkInstantiate", [NetworkMessageInfo]),
        new("OnParticleCollision", [GameObject]),
        new("OnParticleSystemStopped", []),
        new("OnParticleTrigger", []),
        new("OnParticleUpdateJobScheduled", []),
        new("OnPlayerConnected", [NetworkPlayer]),
        new("OnPlayerDisconnected", [NetworkPlayer]),
        new("OnPostRender", []),
        new("OnPreCull", []),
        new("OnPreRender", []),
        new("OnRenderImage", [RenderTexture, RenderTexture]),
        new("OnRenderObject", []),
        new("OnSerializeNetworkView", [BITStream, NetworkMessageInfo]),
        new("OnServerInitialized", []),
        new("OnTransformChildrenChanged", []),
        new("OnTransformParentChanged", []),
        new("OnTriggerEnter", [Collider]),
        new("OnTriggerEnter2D", [Collider2D]),
        new("OnTriggerExit", [Collider]),
        new("OnTriggerExit2D", [Collider2D]),
        new("OnTriggerStay", [Collider]),
        new("OnTriggerStay2D", [Collider2D]),
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

        foreach (var type in types)
        {
            if (!type.IsMonoBehaviour()) continue;

            StaticLogger.Log.LogDebug($"Patching MonoBehaviour type: {type.FullName}");

            // method invoke pause
            foreach (var eventMethodPair in PauseEventMethods)
            {
                var (eventMethodName, eventMethodArgs) = eventMethodPair;

                // try finding method with no parameters
                var eventMethodsMatch = type.GetMethods().Where(x => x.Name == eventMethodName && !x.IsStatic).ToList();
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

                var ignoreRet = il.Create(OpCodes.Ret);
                il.InsertBeforeInstructionReplace(firstInstruction, ignoreRet,
                    InstructionReplaceFixType.ExceptionRanges);

                // is it an IEnumerator
                if (foundMethod.ReturnType.FullName == typeof(IEnumerator).SaneFullName())
                {
                    StaticLogger.Trace($"this method is an IEnumerator");

                    var iEnumeratorRef = assembly.MainModule.ImportReference(typeof(IEnumerator));
                    var rets = il.Body.Instructions.Where(x => x != ignoreRet && x.OpCode == OpCodes.Ret).ToArray();
                    var trackerInvoke = assembly.MainModule.ImportReference(
                        typeof(CoroutineManagerManual).GetMethod(nameof(CoroutineManagerManual.MonoBehNewCoroutine)));

                    var returnTemp = new VariableDefinition(iEnumeratorRef);
                    il.Body.Variables.Add(returnTemp);

                    foreach (var ret in rets)
                    {
                        // 1. dupe return value
                        // 2. store return value
                        // 3. load `this`
                        // 4. load return value
                        // 5. call tracker
                        il.InsertBeforeInstructionReplace(ret, il.Create(OpCodes.Dup));
                        il.InsertBeforeInstructionReplace(ret, il.Create(OpCodes.Stloc, returnTemp));
                        il.InsertBeforeInstructionReplace(ret, il.Create(OpCodes.Ldarg_0));
                        il.InsertBeforeInstructionReplace(ret, il.Create(OpCodes.Ldloc, returnTemp));
                        il.InsertBeforeInstructionReplace(ret, il.Create(OpCodes.Call, trackerInvoke));
                    }
                }

                foundMethod.Body.OptimizeMacros();
            }

            StaticLogger.Trace("Patched Update related methods for skipping execution");

            // event methods invoke
            foreach (var eventMethodPair in EventMethods)
            {
                InvokeUnityEventMethod(type, eventMethodPair.Item1, assembly, eventMethodPair.Item2);
            }
        }
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

        method.Body.Optimize();

        StaticLogger.Trace(
            $"Successfully patched {methodName} for type {type.FullName} for updates, invokes {eventInvoker.Name}");
    }
}