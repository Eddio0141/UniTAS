using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public override IEnumerable<string> TargetDLLs => TargetPatcherDlls.AllDLLs.Where(x =>
    {
        var fileWithoutExtension = Path.GetFileNameWithoutExtension(x);
        return fileWithoutExtension == null ||
               StaticCtorPatchTargetInfo.AssemblyIncludeRaw.Any(a => fileWithoutExtension.Like(a)) ||
               !StaticCtorPatchTargetInfo.AssemblyExclusionsRaw.Any(a => fileWithoutExtension.Like(a));
    });

    public override void Patch(ref AssemblyDefinition assembly)
    {
        StaticLogger.Log.LogDebug("Patching static ctors");

        foreach (var type in assembly.Modules.SelectMany(module => module.GetAllTypes()))
        {
            // remove readonly from all static fields
            StaticLogger.Log.LogDebug($"Removing readonly from static fields in {type.FullName}");
            RemoveReadOnly(type);

            StaticLogger.Log.LogDebug($"Patching static ctor of {type.FullName}");
            var staticCtor = ILCodeUtils.FindOrAddCctor(assembly, type);
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
        ilProcessor.InsertBefore(first, startRefInstruction);
        insertedInstructions.Add(startRefInstruction);
        StaticLogger.Log.LogDebug($"Patched start of static ctor of {type.FullName}");

        var insertCount = 0;

        for (var i = 0; i < instructions.Count; i++)
        {
            var instruction = instructions[i];
            if (instruction.OpCode != OpCodes.Ret) continue;
            var endRefInstruction = ilProcessor.Create(OpCodes.Call, patchMethodEndRef);
            ilProcessor.InsertBefore(instruction, endRefInstruction);
            insertedInstructions.Add(endRefInstruction);
            StaticLogger.Log.LogDebug(
                $"Found return in static ctor of {type.FullName}, instruction: {instruction}, patched");
            ILCodeUtils.RedirectJumpsToNewDest(ilProcessor.Body.Instructions, instruction, endRefInstruction);
            i++;
            insertCount++;
        }

        if (insertCount == 0)
        {
            var last = instructions.Last();
            var endRefInstruction = ilProcessor.Create(OpCodes.Call, patchMethodEndRef);
            ilProcessor.InsertAfter(last, endRefInstruction);
            insertedInstructions.Add(endRefInstruction);
            StaticLogger.Log.LogDebug(
                $"Found no returns in static ctor of {type.FullName}, force patching at last instruction");

            ILCodeUtils.RedirectJumpsToNewDest(ilProcessor.Body.Instructions, last, endRefInstruction);
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

            if (operand is VariableDefinition) continue;

            if (operand is MemberReference m)
            {
                if (m.DeclaringType == null || m.DeclaringType.Equals(type)) continue;

                // ok we found it, insert call to CheckAndInvokeDependency
                StaticLogger.Log.LogDebug("Before insert");
                var dependencyRefInstruction = ilProcessor.Create(OpCodes.Call, patchMethodDependencyRef);
                ilProcessor.InsertBefore(instruction, dependencyRefInstruction);
                insertedDependencyInvoke = true;

                StaticLogger.Log.LogDebug(
                    $"Found external class reference in static ctor of {type.FullName}, instruction: {instruction}, patched");

                ILCodeUtils.RedirectJumpsToNewDest(ilProcessor.Body.Instructions, instruction,
                    dependencyRefInstruction);

                break;
            }

            // default case
            StaticLogger.Log.LogWarning(
                $"Found operand that could be referencing external class, instruction: {instruction}, operand type: {operand.GetType().FullName}");
        }

        if (!insertedDependencyInvoke)
        {
            StaticLogger.Log.LogDebug(
                $"Found no external class reference in static ctor of {type.FullName}, patching to invoke after start of static ctor");

            ilProcessor.InsertAfter(startRefInstruction, ilProcessor.Create(OpCodes.Call, patchMethodDependencyRef));
        }

        staticCtor.Body.OptimizeMacros();
    }
}

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class PatchMethods
{
    private static readonly List<Type> CctorInvokeStack = new();

    // this is how we chain call static ctors, parent and child
    private static readonly Dictionary<Type, List<(Type, MethodBase)>> CctorDependency = new();

    private static readonly List<Type> PendingIgnoreAddingInvokeList = new();

    public static void StaticCtorStart()
    {
        var type = new StackFrame(1).GetMethod()?.DeclaringType;

        if (type == null)
        {
            throw new NullReferenceException("Could not find type of static ctor, something went horribly wrong");
        }

        CctorInvokeStack.Add(type);

        if (IsNotFirstInvoke(type)) return;
        StaticLogger.Log.LogDebug(
            $"First static ctor invoke for {type.FullName}, stack count: {CctorInvokeStack.Count}");

        // first invoke zone

        // find and store static fields for later
        var declaredFields = AccessTools.GetDeclaredFields(type).Where(x => x.IsStatic && !x.IsLiteral);
        ClassStaticInfoTracker.AddStaticFields(declaredFields);

        // if this is chain called, store dependency
        // in this case, we are the child since this is chain called
        if (CctorInvokeStack.Count > 1)
        {
            var parent = CctorInvokeStack[CctorInvokeStack.Count - 2];
            var cctor = AccessTools.DeclaredConstructor(type, searchForStatic: true);

            if (!CctorDependency.ContainsKey(parent))
            {
                CctorDependency.Add(parent, new());
            }

            var list = CctorDependency[parent];
            list.Add(new(type, cctor));

            PendingIgnoreAddingInvokeList.Add(type);

            StaticLogger.Log.LogDebug(
                $"Found static ctor dependency, parent: {parent.FullName}, child: {type.FullName}");
        }
    }

    public static void StaticCtorEnd()
    {
        var type = new StackFrame(1).GetMethod()?.DeclaringType;

        if (type == null)
        {
            throw new NullReferenceException("Could not find type of static ctor, something went horribly wrong");
        }

        CctorInvokeStack.RemoveAt(CctorInvokeStack.Count - 1);
        StaticLogger.Log.LogDebug($"End of static ctor {type}, stack count: {CctorInvokeStack.Count}");

        if (IsNotFirstInvoke(type)) return;

        // add only if not in ignore list
        if (PendingIgnoreAddingInvokeList.Remove(type)) return;

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

        StaticLogger.Log.LogDebug(
            $"Found dependencies for static ctor type: {type.FullName}, dependency count: {dependencies.Count}");
        foreach (var (cctorType, dependency) in dependencies)
        {
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