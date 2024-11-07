using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.ManualServices.Trackers;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Preloader;

public class SerializationCallbackPatch : PreloadPatcher
{
    public override IEnumerable<string> TargetDLLs => TargetPatcherDlls.AllExcludedDLLs;

    public override void Patch(ref AssemblyDefinition assembly)
    {
        var types = assembly.MainModule.GetAllTypes();

        foreach (var type in types)
        {
            if (type.Interfaces.All(i => i.InterfaceType.FullName != "UnityEngine.ISerializationCallbackReceiver"))
                continue;
            var method = type.Methods.First(m =>
                m.Name is "OnAfterDeserialize" or "UnityEngine.ISerializationCallbackReceiver.OnAfterDeserialize");
            if (!method.HasBody) continue;
            method.Body.SimplifyMacros();
            var il = method.Body.GetILProcessor();

            var hook = typeof(SerializationCallbackTracker).GetMethod(
                nameof(SerializationCallbackTracker.OnAfterDeserializeInvoke), AccessTools.all);
            var hookRef = assembly.MainModule.ImportReference(hook);

            var first = method.Body.Instructions.First();
            il.InsertBeforeInstructionReplace(first, il.Create(OpCodes.Ldarg_0));
            il.InsertBeforeInstructionReplace(first, il.Create(OpCodes.Call, hookRef),
                InstructionReplaceFixType.ExceptionRanges);

            method.Body.OptimizeMacros();

            StaticLogger.LogDebug($"Patched `{type}` OnAfterDeserialize");
        }
    }
}