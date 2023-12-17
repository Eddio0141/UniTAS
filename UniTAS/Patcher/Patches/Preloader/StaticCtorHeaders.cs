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
using UniTAS.Patcher.ManualServices.Trackers;
using UniTAS.Patcher.Utils;
using FieldAttributes = Mono.Cecil.FieldAttributes;

namespace UniTAS.Patcher.Patches.Preloader;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class StaticCtorHeaders : PreloadPatcher
{
    public override IEnumerable<string> TargetDLLs => TargetPatcherDlls.AllExcludedDLLs;

    public override void Patch(ref AssemblyDefinition assembly)
    {
        StaticLogger.Log.LogDebug("Patching static ctors");

        foreach (var type in assembly.Modules.SelectMany(module => module.GetAllTypes()))
        {
            // ignore enums
            if (type.IsEnum) continue;

            StaticLogger.Log.LogDebug($"Patching {type.FullName} for readonly fields and cctor");

            // remove readonly from all static fields
            StaticLogger.Trace($"Removing readonly from static fields in {type.FullName}");
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
            StaticLogger.Trace($"Before attributes: {field.Attributes}");
            field.Attributes &= ~FieldAttributes.InitOnly;
            StaticLogger.Trace($"After attributes: {field.Attributes}");
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
        var insertedInstructions = new List<Instruction>();

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

        var insertCount = 0;

        for (var i = 0; i < instructions.Count; i++)
        {
            var retInstruction = instructions[i];
            if (retInstruction.OpCode != OpCodes.Ret) continue;

            var endRefInstruction = ilProcessor.Create(OpCodes.Call, patchMethodEndRef);
            ilProcessor.InsertBeforeInstructionReplace(retInstruction, endRefInstruction);
            insertedInstructions.Add(endRefInstruction);

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
        foreach (var instruction in instructions)
        {
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

        staticCtor.Body.OptimizeMacros();
    }
}

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class PatchMethods
{
    // dictionary here are for different threads
    private static readonly ConcurrentDictionary<int, List<Type>> CctorInvokeStack = new();

    // this is how we chain call static ctors, parent and child
    private static readonly ConcurrentDictionary<Type, List<(Type, MethodBase)>> CctorDependency = new();

    // dictionary here are for different threads
    private static readonly ConcurrentDictionary<int, List<Type>> PendingIgnoreAddingInvokeList = new();

    public static void StaticCtorStart()
    {
        var type = new StackFrame(1).GetMethod()?.DeclaringType;

        if (type == null)
        {
            StaticLogger.Log.LogError("Could not find type of static ctor, something went horribly wrong");
            return;
        }

        var threadId = Thread.CurrentThread.ManagedThreadId;

        var invokeStack = CctorInvokeStack.GetOrAdd(threadId, _ => new());
        invokeStack.Add(type);

        StaticLogger.Log.LogDebug(
            $"Start of static ctor {type.SaneFullName()}, stack count: {invokeStack.Count}, thread id: {threadId}");
        if (IsNotFirstInvoke(type)) return;
        StaticLogger.Trace("First static ctor invoke");

        // first invoke zone

        // find and store static fields for later
        var declaredFields = AccessTools.GetDeclaredFields(type).Where(x => x.IsStatic && !x.IsLiteral);
        ClassStaticInfoTracker.AddStaticFields(declaredFields);

        // if this is chain called, store dependency
        // in this case, we are the child since this is chain called
        if (invokeStack.Count > 1)
        {
            var parent = invokeStack[invokeStack.Count - 2];
            var cctor = AccessTools.DeclaredConstructor(type, searchForStatic: true);

            var list = CctorDependency.GetOrAdd(parent, _ => new());
            list.Add(new(type, cctor));

            var ignoreAddingList = PendingIgnoreAddingInvokeList.GetOrAdd(threadId, _ => new());
            ignoreAddingList.Add(type);

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

        var invokeStack = CctorInvokeStack[threadId];
        invokeStack.RemoveAt(invokeStack.Count - 1);

        StaticLogger.Log.LogDebug(
            $"End of static ctor {type.SaneFullName()}, stack count: {invokeStack.Count}, thread id: {threadId}");
        if (IsNotFirstInvoke(type)) return;

        // add only if not in ignore list
        if (PendingIgnoreAddingInvokeList[threadId].Remove(type)) return;

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