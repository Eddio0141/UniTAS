using System.Linq;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Utils;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.ManualServices.Trackers;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Preloader;

public class SerializationCallbackPatch : PreloadPatcher
{
    public override void Patch(ref AssemblyDefinition assembly)
    {
        var types = assembly.MainModule.GetAllTypes();

        foreach (var type in types)
        {
            if (type.IsInterface || type.IsAbstract) continue;

            var serializationCallback = false;
            foreach (var i in type.Interfaces)
            {
                var iType = i.InterfaceType;
                while (iType != null)
                {
                    if (iType.FullName == "UnityEngine.ISerializationCallbackReceiver")
                    {
                        serializationCallback = true;
                        break;
                    }

                    iType = iType.SafeResolve()?.BaseType;
                }

                if (serializationCallback) break;
            }

            if (!serializationCallback)
                continue;

            var method = type.Methods.FirstOrDefault(m =>
                m.Name is "OnAfterDeserialize" or "UnityEngine.ISerializationCallbackReceiver.OnAfterDeserialize");
            if (method is not { HasBody: true }) continue;
            
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