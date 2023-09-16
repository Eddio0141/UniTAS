using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UniTAS.Patcher.Extensions;
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

        var body = methodDefinition.Body;
        body.SimplifyMacros();
        var ilProcessor = body.GetILProcessor();

        // insert call before first instruction
        if (body.Instructions.Count == 0)
        {
            ilProcessor.Append(ilProcessor.Create(OpCodes.Call, invoke));
        }
        else
        {
            ilProcessor.InsertBeforeInstructionReplace(body.Instructions.First(),
                ilProcessor.Create(OpCodes.Call, invoke), InstructionReplaceFixType.ExceptionRanges);
        }

        body.OptimizeMacros();

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
}