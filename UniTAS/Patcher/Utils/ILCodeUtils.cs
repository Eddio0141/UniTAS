using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UniTAS.Patcher.Extensions;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using MethodBody = Mono.Cecil.Cil.MethodBody;

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
        if (method == null) throw new ArgumentNullException(nameof(method));

        var invoke = assembly.MainModule.ImportReference(method);

        var body = methodDefinition.Body;
        var newBody = body == null || body.Instructions.Count == 0;
        if (newBody)
        {
            methodDefinition.Body = new MethodBody(methodDefinition);
            body = methodDefinition.Body;
        }
        else
        {
            body.SimplifyMacros();
        }

        var ilProcessor = body.GetILProcessor();

        // insert call before first instruction
        if (newBody)
        {
            ilProcessor.Append(ilProcessor.Create(OpCodes.Call, invoke));
        }
        else
        {
            ilProcessor.InsertBeforeInstructionReplace(body.Instructions.First(),
                ilProcessor.Create(OpCodes.Call, invoke), InstructionReplaceFixType.ExceptionRanges);
        }

        if (newBody)
        {
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
        }
        else
        {
            body.OptimizeMacros();
        }

        StaticLogger.Trace(
            $"Added invoke hook to method {method.Name} of {methodDefinition.DeclaringType.FullName} invoking {method.DeclaringType?.FullName ?? "unknown"}.{method.Name}");
    }

    public static MethodDefinition FindOrAddCctor(AssemblyDefinition assembly, TypeDefinition type)
    {
        var staticCtor = type.Methods.FirstOrDefault(m => m.IsConstructor && m.IsStatic);
        if (staticCtor != null) return staticCtor;

        StaticLogger.Trace($"Adding cctor to {type.FullName}");
        staticCtor = new(".cctor",
            MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig
            | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
            assembly.MainModule.ImportReference(typeof(void)));

        type.Methods.Add(staticCtor);
        var il = staticCtor.Body.GetILProcessor();
        il.Append(il.Create(OpCodes.Ret));
        return staticCtor;
    }

    /// <summary>
    /// Returns null or a default value for a struct type
    /// </summary>
    public static void ReturnDefaultValueOnMethod(MethodDefinition method, ILProcessor il)
    {
        var retType = method.ReturnType;

        if (retType.IsValueType)
        {
            var retVar = new VariableDefinition(retType);
            method.Body.Variables.Add(retVar);

            il.Emit(OpCodes.Ldloca, retVar);
            il.Emit(OpCodes.Initobj, retType);
            il.Emit(OpCodes.Ldloc, retVar);
        }
        else
        {
            il.Emit(OpCodes.Ldnull);
        }

        il.Emit(OpCodes.Ret);
    }
}