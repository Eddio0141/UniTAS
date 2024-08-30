using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Preloader;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class UnityInitInvoke : PreloadPatcher
{
    public override IEnumerable<string> TargetDLLs => TargetPatcherDlls.AllExcludedDLLs;

    public override void Patch(ref AssemblyDefinition assembly)
    {
        TryHookUnityEvent(assembly);
        TryHookRuntimeInits(assembly);
    }

    // doing this seems to fail so Awake hooks are enough
    private static void TryHookRuntimeInits(AssemblyDefinition assembly)
    {
        StaticLogger.Log.LogDebug("Trying to hook all RuntimeInitializeOnLoadMethodAttribute methods");

        var types = assembly.MainModule.GetAllTypes();

        foreach (var type in types)
        {
            // find all methods with RuntimeInitializeOnLoadMethodAttribute
            var initMethods = type.GetMethods().Where(x =>
                x.CustomAttributes.Any(a =>
                    a.AttributeType.FullName == "UnityEngine.RuntimeInitializeOnLoadMethodAttribute"));

            foreach (var initMethod in initMethods)
            {
                ILCodeUtils.MethodInvokeHook(assembly, initMethod,
                    AccessTools.Method(typeof(InvokeTracker), nameof(InvokeTracker.OnUnityInit)));
                LogHook(assembly, type.Name, initMethod.Name);
            }
        }
    }

    private static readonly string[] PatchEventMethod =
    [
        "Awake",
        "OnEnable",
        // "Reset", // only for editor
        "Start",
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
        "Update",
        "OnGUI"
    ];

    // cursed, should guarantee loading unitas properly
    private static void TryHookUnityEvent(AssemblyDefinition assembly)
    {
        StaticLogger.Log.LogDebug("Trying to hook MonoBehaviour event methods");

        var types = assembly.MainModule.GetAllTypes().Where(x => x.IsMonoBehaviour());

        foreach (var type in types)
        {
            var methods = type.GetMethods().Where(x => !x.IsStatic && PatchEventMethod.Contains(x.Name));

            foreach (var method in methods)
            {
                ILCodeUtils.MethodInvokeHook(assembly, method,
                    AccessTools.Method(typeof(InvokeTracker), nameof(InvokeTracker.OnUnityInit)));
                LogHook(assembly, type.Name, "Awake");
            }
        }
    }

    private static void LogHook(AssemblyDefinition assembly, string type, string method)
    {
        StaticLogger.Log.LogDebug(
            $"Adding UniTAS init hook for assembly {assembly.Name.Name} to method {type}.{method}");
    }
}