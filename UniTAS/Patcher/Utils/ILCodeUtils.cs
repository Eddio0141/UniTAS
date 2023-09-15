using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace UniTAS.Patcher.Utils;

public static class ILCodeUtils
{
    public static void MethodInvokeHookOnCctor(AssemblyDefinition assembly, TypeDefinition type, MethodBase method)
    {
        if (type == null) return;

        var staticCtor = FindOrAddCctor(assembly, type);
        MethodInvokeHook(assembly, staticCtor, method);
    }

    public static void MethodInvokeHook(AssemblyDefinition assembly, MethodDefinition methodDefinition,
        MethodBase method)
    {
        if (method == null) return;

        var invoke = assembly.MainModule.ImportReference(method);

        methodDefinition.Body.SimplifyMacros();
        var firstInstruction = methodDefinition.Body.Instructions.First();
        var ilProcessor = methodDefinition.Body.GetILProcessor();

        // insert call before first instruction
        ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, invoke));
        methodDefinition.Body.OptimizeMacros();

        StaticLogger.Log.LogDebug(
            $"Added invoke hook to method {method.Name} of {methodDefinition.DeclaringType.FullName} invoking {method.DeclaringType?.FullName ?? "unknown"}.{method.Name}");
    }

    public static MethodDefinition FindOrAddCctor(AssemblyDefinition assembly, TypeDefinition type)
    {
        var staticCtor = type.Methods.FirstOrDefault(m => m.IsConstructor && m.IsStatic);
        if (staticCtor != null) return staticCtor;

        StaticLogger.Log.LogDebug($"Adding cctor to {type.FullName}");
        staticCtor = new(".cctor",
            MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig
            | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
            assembly.MainModule.ImportReference(typeof(void)));

        type.Methods.Add(staticCtor);
        var il = staticCtor.Body.GetILProcessor();
        il.Append(il.Create(OpCodes.Ret));
        return staticCtor;
    }

    public static void RedirectJumpsToNewDest(Collection<Instruction> instructions, Instruction oldDest,
        Instruction newDest)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.Operand == oldDest)
            {
                instruction.Operand = newDest;
                continue;
            }

            // switch
            if (instruction.Operand is Instruction[] instructionsArray)
            {
                for (var i = 0; i < instructionsArray.Length; i++)
                {
                    if (instructionsArray[i] == oldDest)
                    {
                        instructionsArray[i] = newDest;
                    }
                }
            }
        }
    }
}