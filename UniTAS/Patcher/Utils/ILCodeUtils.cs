using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Utils;
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

        if (retType == method.Module.TypeSystem.Void)
        {
            il.Emit(OpCodes.Ret);
            return;
        }

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

    public static void HookHarmony(MethodDefinition method, MethodInfo prefix, MethodInfo postfix)
    {
        if (prefix == null && postfix == null) return;
        if (!method.HasBody) return;

        var body = method.Body;
        body.SimplifyMacros();
        var il = body.GetILProcessor();

        VariableDefinition resultVar = null;
        if (method.ReturnType != method.Module.TypeSystem.Void)
        {
            resultVar = new VariableDefinition(method.ReturnType);
            body.Variables.Add(resultVar);
        }

        // prefix
        VariableDefinition stateDef = null;
        if (prefix != null)
        {
            if (prefix.ReturnType != typeof(bool) && prefix.ReturnType != typeof(void))
                throw new InvalidOperationException("return type for prefix must be bool or void");
            var boolReturn = prefix.ReturnType == typeof(bool);

            var prefixParams = prefix.GetParameters();
            var prefixRef = method.Module.ImportReference(prefix);
            var first = body.Instructions.First();

            if (resultVar != null)
            {
                // default return
                if (resultVar.VariableType.IsValueType)
                {
                    il.InsertBefore(first, il.Create(OpCodes.Ldloca, resultVar));
                    il.InsertBefore(first, il.Create(OpCodes.Initobj, resultVar.VariableType));
                }
                else
                {
                    il.InsertBefore(first, il.Create(OpCodes.Ldnull));
                    il.InsertBefore(first, il.Create(OpCodes.Stloc, resultVar));
                }
            }

            // has state?
            foreach (var param in prefixParams)
            {
                if (param.Name != "__state") continue;

                if (param.ParameterType is { IsValueType: true, IsByRef: false })
                    throw new InvalidOperationException(
                        "state is a value type, but argument isn't using `ref` to be modifiable");
                var paramType = param.ParameterType.HasElementType
                    ? param.ParameterType.GetElementType()!
                    : param.ParameterType;
                var paramTypeImport = method.Module.ImportReference(paramType);
                stateDef = new VariableDefinition(paramTypeImport);
                body.Variables.Add(stateDef);
                // set default
                if (paramType.IsValueType)
                {
                    il.InsertBefore(first, il.Create(OpCodes.Ldloca, stateDef));
                    il.InsertBefore(first, il.Create(OpCodes.Initobj, paramTypeImport));
                }
                else
                {
                    il.InsertBefore(first, il.Create(OpCodes.Ldnull));
                    il.InsertBefore(first, il.Create(OpCodes.Stloc, stateDef));
                }

                break;
            }

            foreach (var param in prefixParams)
            {
                var insertInst = HandleInjection(method, il, param, resultVar,
                    (v, i, p) => i.Create(p.ParameterType.IsByRef ? OpCodes.Ldloca : OpCodes.Ldloc, v), stateDef);
                il.InsertBefore(first, insertInst);
            }

            il.InsertBefore(first, il.Create(OpCodes.Call, prefixRef));
            if (boolReturn)
            {
                il.InsertBefore(first, il.Create(OpCodes.Brtrue, first));
                il.InsertBefore(first, il.Create(OpCodes.Ldloc, resultVar));
            }

            il.InsertBefore(first, il.Create(OpCodes.Ret));
        }

        // postfix
        if (postfix != null)
        {
            if (postfix.ReturnType != typeof(void))
                throw new InvalidOperationException("postfix hook must be void return type");

            var postfixParams = postfix.GetParameters();
            var postfixRef = method.Module.ImportReference(postfix);

            foreach (var inst in body.Instructions.ToArray())
            {
                if (inst.OpCode != OpCodes.Ret) continue;
                if (resultVar != null)
                    il.InsertBeforeInstructionReplace(inst, il.Create(OpCodes.Stloc, resultVar));

                // start pushing args
                foreach (var param in postfixParams)
                {
                    var insertInst = HandleInjection(method, il, param, resultVar, (v, i, _) =>
                    {
                        if (v == null)
                            throw new InvalidOperationException(
                                "Prefix doesn't have a state, add it to prefix or remove from postfix");
                        return i.Create(OpCodes.Ldloc, v);
                    }, stateDef);

                    il.InsertBeforeInstructionReplace(inst, insertInst);
                }

                il.InsertBeforeInstructionReplace(inst, il.Create(OpCodes.Call, postfixRef));

                if (resultVar != null)
                    il.InsertBeforeInstructionReplace(inst, il.Create(OpCodes.Ldloc, resultVar));
            }
        }

        body.Optimize();
    }

    private static Instruction HandleInjection(MethodDefinition method, ILProcessor il, ParameterInfo param,
        VariableDefinition resultVar, Func<VariableDefinition, ILProcessor, ParameterInfo, Instruction> state,
        VariableDefinition stateVar)
    {
        var name = param.Name;
        switch (name)
        {
            // check for injections
            case "__instance":
            {
                if (method.IsStatic)
                    throw new InvalidOperationException(
                        "Hook requested for __instance param, but this is a static method");
                return il.Create(OpCodes.Ldarg_0);
            }
            case "__result":
            {
                if (resultVar == null)
                    throw new InvalidOperationException(
                        "Hook requested for __result param, but return type is void");
                return param.ParameterType.IsByRef
                    ? il.Create(OpCodes.Ldloca, resultVar)
                    : il.Create(OpCodes.Ldloc, resultVar);
            }
            case "__state":
            {
                return state(stateVar, il, param);
            }
            default:
            {
                // method arg, does name match
                var paramDef = method.Parameters.FirstOrDefault(x => x.Name == name);
                if (paramDef == null)
                    throw new InvalidOperationException(
                        $"Hook requested for non-existent method param name `{name}`");
                return il.Create(OpCodes.Ldarg, paramDef);
            }
        }
    }
}