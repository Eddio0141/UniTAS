using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using BepInEx;
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
    private const string RUNTIME_INITIALIZE_LOAD_TYPE = "UnityEngine.RuntimeInitializeLoadType";
    private const string BEFORE_SCENE_LOAD_VARIANT = "BeforeSceneLoad";

    public override IEnumerable<string> TargetDLLs => TargetPatcherDlls.AllExcludedDLLs;

    private readonly object _beforeSceneLoadValue;

    public UnityInitInvoke()
    {
        var unityCoreModulePath = Path.Combine(Paths.ManagedPath, "UnityEngine.CoreModule.dll");
        if (!File.Exists(unityCoreModulePath))
        {
            NotifyBeforeSceneLoadNotFound("UnityEngine.CoreModule.dll not found");
            return;
        }

        var asm = AssemblyDefinition.ReadAssembly(unityCoreModulePath);

        var loadType = asm.MainModule.Types.FirstOrDefault(x => x.FullName == RUNTIME_INITIALIZE_LOAD_TYPE);
        if (loadType == null)
        {
            NotifyBeforeSceneLoadNotFound($"{RUNTIME_INITIALIZE_LOAD_TYPE} not found");
            return;
        }

        var beforeSceneLoadField = loadType.Fields.FirstOrDefault(x => x.Name == BEFORE_SCENE_LOAD_VARIANT);
        if (beforeSceneLoadField == null)
        {
            NotifyBeforeSceneLoadNotFound(
                $"{RUNTIME_INITIALIZE_LOAD_TYPE}.{BEFORE_SCENE_LOAD_VARIANT} enum variant not found");
            return;
        }

        _beforeSceneLoadValue = beforeSceneLoadField.Constant;
    }

    private static void NotifyBeforeSceneLoadNotFound(string reason)
    {
        StaticLogger.LogDebug($"couldn't obtain {RUNTIME_INITIALIZE_LOAD_TYPE}.{BEFORE_SCENE_LOAD_VARIANT}, {reason}");
    }

    public override void Patch(ref AssemblyDefinition assembly)
    {
        TryHookUnityEvent(assembly);
        TryHookRuntimeInits(assembly);
    }

    // doing this seems to fail so Awake hooks are enough
    private void TryHookRuntimeInits(AssemblyDefinition assembly)
    {
        if (_beforeSceneLoadValue == null) return;

        StaticLogger.Log.LogDebug(
            "Trying to hook all RuntimeInitializeOnLoadMethodAttribute methods with BeforeSceneLoad");

        var types = assembly.MainModule.GetAllTypes();

        foreach (var type in types)
        {
            // find all methods with RuntimeInitializeOnLoadMethodAttribute
            var initMethods = type.GetMethods().Where(x =>
                x.CustomAttributes.Any(a =>
                    a.AttributeType.FullName == "UnityEngine.RuntimeInitializeOnLoadMethodAttribute" &&
                    a.HasConstructorArguments && a.ConstructorArguments.Count == 1 &&
                    a.ConstructorArguments[0].Type.FullName == RUNTIME_INITIALIZE_LOAD_TYPE &&
                    (int)a.ConstructorArguments[0].Value == (int)_beforeSceneLoadValue));

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