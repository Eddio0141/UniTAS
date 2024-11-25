using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.ManualServices;
using UniTAS.Patcher.ManualServices.Trackers;
using UniTAS.Patcher.Utils;
using FieldAttributes = Mono.Cecil.FieldAttributes;

namespace UniTAS.Patcher.Patches.Preloader;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class StaticCtorHeaders : PreloadPatcher
{
    private readonly HashSet<string> _ignoreTypes =
        ["UnityEngine.Experimental.Rendering.ScriptableRuntimeReflectionSystemSettings"];

    public override void Patch(ref AssemblyDefinition assembly)
    {
        StaticLogger.Log.LogDebug("Patching static ctors");

        foreach (var type in assembly.Modules.SelectMany(module => module.GetAllTypes()))
        {
            // ignore enums and interfaces
            if (type.IsEnum || type.IsInterface) continue;

            var typeFullName = type.FullName;

            if (_ignoreTypes.Remove(typeFullName)) continue;

            StaticLogger.Log.LogDebug($"Patching {typeFullName} for readonly fields and cctor");

            // remove readonly from all static fields
            StaticLogger.Trace($"Removing readonly from static fields in {typeFullName}");
            RemoveReadOnly(type);

            // we need to add a static ctor as it will be responsible for tracking and resetting static fields
            var staticCtor = ILCodeUtils.FindOrAddCctor(assembly, type);
            StaticLogger.Trace("Patching static ctor");
            PatchStaticCtor(assembly, staticCtor, type);
        }
    }

    /// <summary>
    /// Removes "readonly" from all fields
    /// </summary>
    private static void RemoveReadOnly(TypeDefinition type)
    {
        foreach (var field in type.Fields)
        {
            if (field.IsLiteral || !field.IsStatic) continue;

            StaticLogger.Trace($"Removing readonly from field {field.Name}");
            field.Attributes &= ~FieldAttributes.InitOnly;
        }
    }

    /// <summary>
    /// Patch the end or all returns of the static ctor for our own purposes
    /// </summary>
    private static void PatchStaticCtor(AssemblyDefinition assembly, MethodDefinition staticCtor, TypeDefinition type)
    {
        var patchMethodStart = AccessTools.Method(typeof(PatchMethods), nameof(PatchMethods.StaticCtorStart));
        var patchMethodEnd = AccessTools.Method(typeof(PatchMethods), nameof(PatchMethods.StaticCtorEnd));
        var patchMethodStartRef = assembly.MainModule.ImportReference(patchMethodStart);
        var patchMethodEndRef = assembly.MainModule.ImportReference(patchMethodEnd);
        var patchMethodDependency =
            AccessTools.Method(typeof(PatchMethods), nameof(PatchMethods.CheckAndInvokeDependency));
        var patchMethodDependencyRef = assembly.MainModule.ImportReference(patchMethodDependency);

        // we track our inserted instructions
        var insertedInstructions = new HashSet<Instruction>();

        // we insert call before any returns
        staticCtor.Body.SimplifyMacros();
        var ilProcessor = staticCtor.Body.GetILProcessor();
        var instructions = staticCtor.Body.Instructions;
        var first = instructions.First();

        // insert call to our method at the start of the static ctor
        var startRefInstruction = ilProcessor.Create(OpCodes.Call, patchMethodStartRef);
        ilProcessor.InsertBeforeInstructionReplace(first, startRefInstruction,
            InstructionReplaceFixType.ExceptionRanges);
        insertedInstructions.Add(startRefInstruction);
        StaticLogger.Trace($"Patched start of static ctor of {type.FullName}");

        var finalRet = ilProcessor.Create(OpCodes.Ret);

        var insertCount = 0;

        for (var i = 0; i < instructions.Count; i++)
        {
            var retInstruction = instructions[i];
            if (retInstruction.OpCode != OpCodes.Ret) continue;

            var endRefInstruction = ilProcessor.Create(OpCodes.Call, patchMethodEndRef);
            ilProcessor.InsertBeforeInstructionReplace(retInstruction, endRefInstruction);
            insertedInstructions.Add(endRefInstruction);
            ilProcessor.Replace(retInstruction, ilProcessor.Create(OpCodes.Leave, finalRet));

            StaticLogger.Trace(
                $"Found return in static ctor of {type.FullName}, instruction: {retInstruction}, patched");

            i++;
            insertCount++;
        }

        if (insertCount == 0)
        {
            var endRefInstruction = ilProcessor.Create(OpCodes.Call, patchMethodEndRef);
            ilProcessor.InsertBeforeInstructionReplace(instructions.Last(), endRefInstruction);
            insertedInstructions.Add(endRefInstruction);
            StaticLogger.Trace(
                $"Found no returns in static ctor of {type.FullName}, force patching at last instruction");
        }

        // i gotta find a nice place to insert call to CheckAndInvokeDependency
        // for this i can just analyze the static ctor and find the first external class

        var insertedDependencyInvoke = false;
        for (var i = 0; i < instructions.Count; i++)
        {
            var instruction = instructions[i];
            // ignore our own inserted instructions
            if (insertedInstructions.Contains(instruction)) continue;

            var operand = instruction.Operand;
            if (operand == null) continue;

            // don't bother with mscorlib
            if (Equals(operand.GetType().Assembly, typeof(string).Assembly)) continue;

            if (operand is VariableDefinition or Instruction) continue;

            if (operand is MemberReference m)
            {
                if (m.DeclaringType == null || m.DeclaringType.Equals(type)) continue;

                // ok we found it, insert call to CheckAndInvokeDependency
                StaticLogger.Trace("Before insert");
                var dependencyRefInstruction = ilProcessor.Create(OpCodes.Call, patchMethodDependencyRef);

                // BEFORE WE DO THIS... ensure opcode `constrained` is handled
                // sandwiching my method between `constrained` and a `callvirt` will BREAK STUFF
                if (i > 0 && instructions[i - 1].OpCode == OpCodes.Constrained)
                    ilProcessor.InsertBeforeInstructionReplace(instructions[i - 1], dependencyRefInstruction);
                else
                    ilProcessor.InsertBeforeInstructionReplace(instruction, dependencyRefInstruction);

                insertedDependencyInvoke = true;

                StaticLogger.Trace(
                    $"Found external class reference in static ctor of {type.FullName}, instruction: {instruction}, patched");

                break;
            }

            // default case
            StaticLogger.Log.LogWarning(
                $"Found operand that could be referencing external class, instruction: {instruction}, operand type: {operand.GetType().FullName}");
        }

        if (!insertedDependencyInvoke)
        {
            StaticLogger.Trace(
                $"Found no external class reference in static ctor of {type.FullName}, patching to invoke after start of static ctor");

            ilProcessor.InsertAfter(startRefInstruction, ilProcessor.Create(OpCodes.Call, patchMethodDependencyRef));
        }

        // surround the cctor with catch, otherwise exceptions will break tracking and I HATE EXCEPTIONS
        // also I know I'm surrounding everything here, it should be fine!!!
        var last = instructions.Last();
        var ex = new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            CatchType = assembly.MainModule.ImportReference(typeof(Exception)),
            TryStart = instructions.First(),
        };

        // handler, the order of instruction is flipped since InsertAfter
        var exHandlerInst = ilProcessor.Create(OpCodes.Call, patchMethodEndRef);
        var rethrowInst = ilProcessor.Create(OpCodes.Rethrow);

        ilProcessor.InsertAfter(last, rethrowInst);
        ilProcessor.InsertAfter(last, exHandlerInst);
        ilProcessor.Append(finalRet);

        ex.TryEnd = exHandlerInst;
        ex.HandlerStart = exHandlerInst;
        ex.HandlerEnd = finalRet;

        staticCtor.Body.ExceptionHandlers.Add(ex);

        staticCtor.Body.OptimizeMacros();
    }
}

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class PatchMethods
{
    private static readonly ThreadLocal<List<Type>> CctorInvokeStack = new(() => new());

    // this is how we chain call static ctors, parent and child
    private static readonly ConcurrentDictionary<Type, List<(Type, MethodBase)>> CctorDependency = new();

    // dictionary here are for different threads
    private static readonly ThreadLocal<List<Type>> PendingIgnoreAddingInvokeList = new(() => new());

    private static readonly ConcurrentDictionary<string, HashSet<string>> FieldIgnoreListConditional = new();

    static PatchMethods()
    {
        // add this if no graphics (very silly)
        if (GameInfoManual.NoGraphics)
            FieldIgnoreListConditional.TryAdd("UnityEngine.Rendering.OnDemandRendering", ["m_RenderFrameInterval"]);
    }

    public static void StaticCtorStart()
    {
        var type = new StackFrame(1).GetMethod()?.DeclaringType;

        if (type == null)
        {
            StaticLogger.Log.LogError("Could not find type of static ctor, something went horribly wrong");
            return;
        }

        var invokeStack = CctorInvokeStack.Value;
        invokeStack.Add(type);

        var stackCount = invokeStack.Count;
        var typeSaneFullName = type.SaneFullName();
        StaticLogger.Log.LogDebug(
            $"Start of static ctor {typeSaneFullName}, stack count: {stackCount}, thread id: {Thread.CurrentThread.ManagedThreadId}");
        if (IsNotFirstInvoke(type)) return;
        StaticLogger.Trace("First static ctor invoke");

        // first invoke zone

        // find and store static fields for later
        var declaredFields = AccessTools.GetDeclaredFields(type).Where(x => x.IsStatic && !x.IsLiteral);
        if (FieldIgnoreListConditional.TryRemove(typeSaneFullName, out var ignoreFields))
        {
            declaredFields = declaredFields.Where(x => !ignoreFields.Contains(x.Name));
        }

        ClassStaticInfoTracker.AddStaticFields(declaredFields);

        // if this is chain called, store dependency
        // in this case, we are the child since this is chain called
        if (stackCount > 1)
        {
            var parent = invokeStack[stackCount - 2];
            var cctor = AccessTools.DeclaredConstructor(type, searchForStatic: true);

            var list = CctorDependency.GetOrAdd(parent, _ => new());
            list.Add(new(type, cctor));

            PendingIgnoreAddingInvokeList.Value.Add(type);

            StaticLogger.Trace($"Found static ctor dependency, parent: {parent.FullName}, child: {type.FullName}");
        }
    }

    public static void StaticCtorEnd()
    {
        var type = new StackFrame(1).GetMethod()?.DeclaringType;

        if (type == null)
        {
            throw new NullReferenceException("Could not find type of static ctor, something went horribly wrong");
        }

        var threadId = Thread.CurrentThread.ManagedThreadId;

        StaticLogger.Log.LogDebug($"End of static ctor {type.SaneFullName()}, thread id: {threadId}");

        var invokeStack = CctorInvokeStack.Value;
        invokeStack.RemoveAt(invokeStack.Count - 1);

        StaticLogger.Log.LogDebug($"stack count: {invokeStack.Count}");

        if (IsNotFirstInvoke(type)) return;

        // add only if not in ignore list
        if (PendingIgnoreAddingInvokeList.Value.Remove(type)) return;

        StaticLogger.Log.LogDebug($"Adding type {type} to static ctor invoke list");
        ClassStaticInfoTracker.AddStaticCtorForTracking(type);
    }

    public static void CheckAndInvokeDependency()
    {
        var type = new StackFrame(1).GetMethod()?.DeclaringType;

        if (type == null)
        {
            throw new NullReferenceException("Could not find type of static ctor, something went horribly wrong");
        }

        if (!IsNotFirstInvoke(type)) return;

        if (!CctorDependency.TryGetValue(type, out var dependencies)) return;

        var dependenciesCount = dependencies.Count;
        StaticLogger.Log.LogDebug(
            $"Found dependencies for static ctor type: {type.FullName}, dependency count: {dependenciesCount}");
        for (var i = 0; i < dependenciesCount; i++)
        {
            var (cctorType, dependency) = dependencies[i];
            StaticLogger.Log.LogDebug($"Invoking cctor of {cctorType.FullName ?? "unknown type"}");
            dependency.Invoke(null, null);
        }
    }

    private static bool IsNotFirstInvoke(Type type)
    {
        if (ClassStaticInfoTracker.StaticCtorInvokeOrder.Contains(type))
            return true;

        foreach (var pair in CctorDependency)
        {
            foreach (var childPair in pair.Value)
            {
                if (childPair.Item1 == type)
                    return true;
            }
        }

        return false;
    }
}