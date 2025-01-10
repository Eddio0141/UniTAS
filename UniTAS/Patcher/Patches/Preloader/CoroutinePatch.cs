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
            if (type.Interfaces.All(x => x.InterfaceType.FullName != typeof(IEnumerator).FullName)) continue;

            var moveNext = type.Methods.First(x => x.Name == "MoveNext" && x.Parameters.Count == 0);
            var current = (type.Properties.FirstOrDefault(x => x.Name == "System.Collections.IEnumerator.Current") ??
                           type.Properties.First(x => x.Name == "Current")).GetMethod;
            if (!moveNext.HasBody || !current.HasBody) continue;

            StaticLogger.LogDebug($"coroutine patch: patching type {type.FullName}");

            var moveNextPrefixTarget =
                typeof(CoroutineManagerManual).GetMethod(nameof(CoroutineManagerManual.CoroutineMoveNextPrefix));
            var currentPostfixTarget =
                typeof(CoroutineManagerManual).GetMethod(nameof(CoroutineManagerManual.CoroutineCurrentPostfix));

            var moveNextPrefix = assembly.MainModule.ImportReference(moveNextPrefixTarget);
            var currentPostfix = assembly.MainModule.ImportReference(currentPostfixTarget);

            // MoveNext
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

            // get_Current
            body = current.Body;
            body.SimplifyMacros();
            il = body.GetILProcessor();
            resultVar = new VariableDefinition(assembly.MainModule.TypeSystem.Object);
            body.Variables.Add(resultVar);

            foreach (var inst in body.Instructions.ToArray())
            {
                if (inst.OpCode != OpCodes.Ret) continue;
                il.InsertBeforeInstructionReplace(inst, il.Create(OpCodes.Stloc, resultVar));
                il.InsertBeforeInstructionReplace(inst, il.Create(OpCodes.Ldarg_0));
                il.InsertBeforeInstructionReplace(inst, il.Create(OpCodes.Ldloca, resultVar));
                il.InsertBeforeInstructionReplace(inst, il.Create(OpCodes.Call, currentPostfix));
                il.InsertBeforeInstructionReplace(inst, il.Create(OpCodes.Ldloc, resultVar));
            }

            body.Optimize();
        }
    }
}