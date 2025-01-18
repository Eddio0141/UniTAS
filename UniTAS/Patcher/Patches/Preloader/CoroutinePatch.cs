using System.Collections;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.ManualServices;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Preloader;

public class CoroutinePatch : PreloadPatcher
{
    public override void Patch(ref AssemblyDefinition assembly)
    {
        var types = assembly.MainModule.GetAllTypes();
        foreach (var type in types)
        {
            if (type.IsValueType) continue;
            if (type.Interfaces.All(x => x.InterfaceType.FullName != typeof(IEnumerator).FullName)) continue;

            StaticLogger.LogDebug($"coroutine patch: patching type {type.FullName}");

            var moveNext = type.Methods.FirstOrDefault(x =>
                x.Name is "MoveNext" or "System.Collections.IEnumerator.MoveNext" &&
                x.Parameters.Count == 0);
            var current =
                type.Properties.FirstOrDefault(x => x.Name is "System.Collections.IEnumerator.Current" or "Current")
                    ?.GetMethod;

            // MoveNext
            if (moveNext?.HasBody is true)
            {
                var moveNextPrefix = assembly.MainModule.ImportReference(
                    typeof(CoroutineManagerManual).GetMethod(nameof(CoroutineManagerManual.CoroutineMoveNextPrefix)));

                var body = moveNext.Body;
                body.SimplifyMacros();
                var il = body.GetILProcessor();
                var first = body.Instructions.First();
                var resultVar = new VariableDefinition(assembly.MainModule.TypeSystem.Boolean);
                body.Variables.Add(resultVar);
                // TODO: create a utility where I can use harmony-type hooks but with preload patcher
                il.InsertBefore(first, il.Create(OpCodes.Ldc_I4_0));
                il.InsertBefore(first, il.Create(OpCodes.Stloc, resultVar));
                il.InsertBefore(first, il.Create(OpCodes.Ldarg_0));
                il.InsertBefore(first, il.Create(OpCodes.Ldloca, resultVar));
                il.InsertBefore(first, il.Create(OpCodes.Call, moveNextPrefix));
                il.InsertBefore(first, il.Create(OpCodes.Brtrue, first));
                il.InsertBefore(first, il.Create(OpCodes.Ldloc, resultVar));
                il.InsertBefore(first, il.Create(OpCodes.Ret));
                body.Optimize();
            }

            if (current?.HasBody is not true) continue;

            var currentPostfix = assembly.MainModule.ImportReference(
                typeof(CoroutineManagerManual).GetMethod(nameof(CoroutineManagerManual.CoroutineCurrentPostfix)));

            // get_Current
            var body2 = current.Body;
            body2.SimplifyMacros();
            var il2 = body2.GetILProcessor();
            var resultVar2 = new VariableDefinition(assembly.MainModule.TypeSystem.Object);
            body2.Variables.Add(resultVar2);

            foreach (var inst in body2.Instructions.ToArray())
            {
                if (inst.OpCode != OpCodes.Ret) continue;
                il2.InsertBeforeInstructionReplace(inst, il2.Create(OpCodes.Stloc, resultVar2));
                il2.InsertBeforeInstructionReplace(inst, il2.Create(OpCodes.Ldarg_0));
                il2.InsertBeforeInstructionReplace(inst, il2.Create(OpCodes.Ldloca, resultVar2));
                il2.InsertBeforeInstructionReplace(inst, il2.Create(OpCodes.Call, currentPostfix));
                il2.InsertBeforeInstructionReplace(inst, il2.Create(OpCodes.Ldloc, resultVar2));
            }

            body2.Optimize();
        }
    }
}