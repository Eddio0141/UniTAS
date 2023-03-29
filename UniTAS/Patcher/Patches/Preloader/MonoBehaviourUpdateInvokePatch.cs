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

public class MonoBehaviourUpdateInvokePatch : PreloadPatcher
{
    public override IEnumerable<string> TargetDLLs => Utils.AllTargetDllsWithGenericExclusions;

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

    public override void Patch(ref AssemblyDefinition assembly)
    {
        var types = assembly.Modules.SelectMany(m => m.GetAllTypes());

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

            foreach (var eventMethodPair in EventMethods)
            {
                InvokeUnityEventMethod(type, eventMethodPair.Key, assembly, eventMethodPair.Value);
            }
        }
    }

    private static void InvokeUnityEventMethod(TypeDefinition type, string methodName, AssemblyDefinition assembly,
        MethodBase eventInvoker)
    {
        Trace.Write($"Attempting to patch {methodName} for type {type.FullName}");
        var method = type.Methods.FirstOrDefault(m => !m.IsStatic && m.Name == methodName && !m.HasParameters);
        if (method == null) return;

        var ilProcessor = method.Body.GetILProcessor();
        var reference = assembly.MainModule.ImportReference(eventInvoker);

        ilProcessor.InsertBefore(method.Body.Instructions[0], ilProcessor.Create(OpCodes.Call, reference));

        Trace.Write($"Successfully patched");
    }
}