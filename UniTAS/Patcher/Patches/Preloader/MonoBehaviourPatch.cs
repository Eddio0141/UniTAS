using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Utils;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Preloader;

public class MonoBehaviourPatch : PreloadPatcher
{
    private readonly string[] _assemblyExclusionsRaw =
    {
        "UnityEngine.*",
        "UnityEngine",
        "Unity.*",
        "System.*",
        "System",
        "netstandard",
        "mscorlib",
        "Mono.*",
        "Mono",
        "MonoMod.*",
        "BepInEx.*",
        "BepInEx",
        "MonoMod.*",
        "0Harmony",
        "HarmonyXInterop",
        "StructureMap",
        "Newtonsoft.Json"
    };

    private readonly string[] _assemblyIncludeRaw =
    {
        "Unity.InputSystem",
        "UnityEngine.InputModule"
    };

    public override IEnumerable<string> TargetDLLs => TargetPatcherDlls.AllDLLs.Where(x =>
    {
        var fileWithoutExtension = Path.GetFileNameWithoutExtension(x);
        return fileWithoutExtension == null ||
               _assemblyIncludeRaw.Any(a => fileWithoutExtension.Like(a)) ||
               !_assemblyExclusionsRaw.Any(a => fileWithoutExtension.Like(a));
    });

    private static readonly KeyValuePair<string, MethodBase>[] EventMethods =
    {
        new("Awake",
            AccessTools.Method(typeof(MonoBehaviourEvents), nameof(MonoBehaviourEvents.InvokeAwake))),
        new("OnEnable",
            AccessTools.Method(typeof(MonoBehaviourEvents), nameof(MonoBehaviourEvents.InvokeOnEnable))),
        new("Start",
            AccessTools.Method(typeof(MonoBehaviourEvents), nameof(MonoBehaviourEvents.InvokeStart))),
        new("Update",
            AccessTools.Method(typeof(MonoBehaviourEvents), nameof(MonoBehaviourEvents.InvokeUpdate))),
        new("LateUpdate",
            AccessTools.Method(typeof(MonoBehaviourEvents), nameof(MonoBehaviourEvents.InvokeLateUpdate))),
        new("FixedUpdate",
            AccessTools.Method(typeof(MonoBehaviourEvents), nameof(MonoBehaviourEvents.InvokeFixedUpdate)))
        // new("OnGUI", AccessTools.Method(typeof(MonoBehaviourEvents), nameof(MonoBehaviourEvents.InvokeOnGUI)))
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
            var pauseMethods = type.GetMethods().Where(x => !x.IsStatic && x.HasBody);

            foreach (var pauseMethod in pauseMethods)
            {
                StaticLogger.Log.LogDebug($"Patching method for pausing execution {pauseMethod.FullName}");

                pauseMethod.Body.SimplifyMacros();
                var il = pauseMethod.Body.GetILProcessor();
                var firstInstruction = il.Body.Instructions.First();

                // return early check
                il.InsertBefore(firstInstruction, il.Create(OpCodes.Call, pauseExecutionReference));
                il.InsertBefore(firstInstruction, il.Create(OpCodes.Brfalse_S, firstInstruction));

                // if the return type isn't void, we need to return a default value
                if (pauseMethod.ReturnType != assembly.MainModule.TypeSystem.Void)
                {
                    // if value type, we need to return a default value
                    if (pauseMethod.ReturnType.IsValueType)
                    {
                        var local = new VariableDefinition(pauseMethod.ReturnType);
                        il.Body.Variables.Add(local);
                        il.InsertBefore(firstInstruction, il.Create(OpCodes.Ldloca_S, local));
                        il.InsertBefore(firstInstruction, il.Create(OpCodes.Initobj, pauseMethod.ReturnType));
                        il.InsertBefore(firstInstruction, il.Create(OpCodes.Ldloc_S, local));
                    }
                    else
                    {
                        il.InsertBefore(firstInstruction, il.Create(OpCodes.Ldnull));
                    }
                }

                il.InsertBefore(firstInstruction, il.Create(OpCodes.Ret));

                pauseMethod.Body.OptimizeMacros();
            }

            // event methods invoke
            foreach (var eventMethodPair in EventMethods)
            {
                InvokeUnityEventMethod(type, eventMethodPair.Key, assembly, eventMethodPair.Value);
            }

            // update skip check
            var updateMethod = type.Methods.FirstOrDefault(m => m.Name == "Update" && !m.HasParameters);
            if (updateMethod is not { HasBody: true }) continue;

            updateMethod.Body.SimplifyMacros();
            var updateIl = updateMethod.Body.GetILProcessor();
            var updateFirstInstruction = updateIl.Body.Instructions.First();

            // return early check
            updateIl.InsertBefore(updateFirstInstruction, updateIl.Create(OpCodes.Call, pausedUpdateReference));
            updateIl.InsertBefore(updateFirstInstruction, updateIl.Create(OpCodes.Brfalse_S, updateFirstInstruction));

            // if the return type isn't void, we need to return a default value
            if (updateMethod.ReturnType != assembly.MainModule.TypeSystem.Void)
            {
                updateIl.InsertBefore(updateFirstInstruction, updateIl.Create(OpCodes.Ldnull));
            }

            updateIl.InsertBefore(updateFirstInstruction, updateIl.Create(OpCodes.Ret));

            updateMethod.Body.OptimizeMacros();

            StaticLogger.Log.LogDebug("Patched Update method for skipping execution");
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

        ilProcessor.InsertBefore(method.Body.Instructions.First(), ilProcessor.Create(OpCodes.Call, reference));

        method.Body.OptimizeMacros();

        StaticLogger.Log.LogDebug(
            $"Successfully patched {methodName} for type {type.FullName} for updates, invokes {eventInvoker.Name}");
    }
}