using System.Collections.Generic;
using System.Reflection;
using HarmonyLib.Public.Patching;

namespace UniTAS.Patcher.ManualServices;

// this literally only exists for the static transpiler method to easily access those things
public static class PatchReverseInvokerManual
{
    private static readonly List<MethodInfo> ReversePatcherMethods = new();

    public static MethodInfo RecursiveReversePatch(MethodInfo original)
    {
        var copiedMethod = original.GetMethodPatcher().CopyOriginal();

        var il = copiedMethod.GetILProcessor();
        var instructions = il.Body.Instructions;
        foreach (var instruction in instructions)
        {
            if (instruction.Operand is not MethodInfo methodInfo) continue;
            if (ReversePatcherMethods.Contains(methodInfo)) continue;
            var patchedMethod = RecursiveReversePatch(methodInfo);
            instruction.Operand = patchedMethod;
        }

        var generated = copiedMethod.Generate();
        ReversePatcherMethods.Add(generated);
        return generated;
    }
}