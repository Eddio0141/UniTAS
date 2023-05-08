using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.Shared;

namespace UniTAS.Patcher.Patches.Preloader;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class StaticCtorHeaders : PreloadPatcher
{
    private readonly string[] _assemblyExclusionsRaw =
    {
        "UnityEngine.*",
        "UnityEngine",
        "Unity.*",
        "System.*",
        "System",
        "netstandard",
        "mscorlib",
        "Mono.*",
        "Mono",
        "MonoMod.*",
        "BepInEx.*",
        "BepInEx",
        "MonoMod.*",
        "0Harmony",
        "HarmonyXInterop",
        "StructureMap"
    };

    private readonly string[] _assemblyEnforceRaw =
    {
        "Unity.InputSystem"
    };

    public override IEnumerable<string> TargetDLLs => Utils.AllTargetDllsWithGenericExclusions;

    public override void Patch(ref AssemblyDefinition assembly)
    {
        var definition = assembly;
        var assemblyName = definition.Name.Name;
        if (_assemblyExclusionsRaw.Any(x => assemblyName.Like(x)) &&
            !_assemblyEnforceRaw.Any(x => assemblyName.Like(x)))
        {
            Patcher.Logger.LogDebug($"Skipping static ctor patching for {definition.Name.Name}");
            return;
        }

        // get all types in assembly that has static fields or a static ctor
        var types = assembly.Modules.SelectMany(m => m.GetAllTypes())
            .Where(t => t.HasFields && t.Fields.Any(f => f.IsStatic && !f.IsLiteral) ||
                        t.HasMethods && t.Methods.Any(m => m.IsStatic && m.IsConstructor));

        Patcher.Logger.LogDebug("Patching static ctors");
        foreach (var type in types)
        {
            // find static ctor
            var staticCtor = type.Methods.FirstOrDefault(m => m.IsConstructor && m.IsStatic);
            // add static ctor if not found
            if (staticCtor == null)
            {
                Patcher.Logger.LogDebug($"Adding static ctor to {type.FullName}");
                staticCtor = new(".cctor",
                    MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig
                    | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                    assembly.MainModule.ImportReference(typeof(void)));

                type.Methods.Add(staticCtor);
                var il = staticCtor.Body.GetILProcessor();
                il.Append(il.Create(OpCodes.Ret));
            }

            Patcher.Logger.LogDebug($"Patching static ctor of {type.FullName}");
            PatchStaticCtor(assembly, staticCtor);
        }
    }

    /// <summary>
    /// Patch the end or all returns of the static ctor for our own purposes
    /// </summary>
    private static void PatchStaticCtor(AssemblyDefinition assembly, MethodDefinition staticCtor)
    {
        var patchMethodStart = AccessTools.Method(typeof(PatchMethods), nameof(PatchMethods.StaticCtorStart));
        var patchMethodEnd = AccessTools.Method(typeof(PatchMethods), nameof(PatchMethods.StaticCtorEnd));
        var patchMethodStartRef = assembly.MainModule.ImportReference(patchMethodStart);
        var patchMethodEndRef = assembly.MainModule.ImportReference(patchMethodEnd);

        // we insert call before any returns
        var ilProcessor = staticCtor.Body.GetILProcessor();
        var instructions = staticCtor.Body.Instructions;
        var first = instructions.First();

        // insert call to our method at the start of the static ctor
        ilProcessor.InsertBefore(first, ilProcessor.Create(OpCodes.Call, patchMethodStartRef));
        Patcher.Logger.LogDebug($"Patched start of static ctor of {staticCtor.DeclaringType?.FullName}");

        var insertCount = 0;

        for (var i = 0; i < instructions.Count; i++)
        {
            var instruction = instructions[i];
            if (instruction.OpCode != OpCodes.Ret) continue;
            ilProcessor.InsertBefore(instruction, ilProcessor.Create(OpCodes.Call, patchMethodEndRef));
            Patcher.Logger.LogDebug($"Found return in static ctor of {staticCtor.DeclaringType?.FullName}, patched");
            i++;
            insertCount++;
        }

        if (insertCount == 0)
        {
            var last = instructions.Last();
            ilProcessor.InsertAfter(last, ilProcessor.Create(OpCodes.Call, patchMethodEndRef));
            Patcher.Logger.LogDebug(
                $"Found no returns in static ctor of {staticCtor.DeclaringType?.FullName}, force patching at last instruction");
        }
    }
}

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class PatchMethods
{
    // re-enable this if a mysterious memory leak / unexplainable hard crashes occurs
    /*
    private static int _resetFieldCount;
    private const int MaxResetFieldCount = 100;

    private static int ResetFieldCount
    {
        get => _resetFieldCount;
        set
        {
            _resetFieldCount = value;
            if (_resetFieldCount > MaxResetFieldCount)
            {
                _resetFieldCount = 0;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
    */

    public static void StaticCtorStart()
    {
        var type = new StackFrame(1).GetMethod()?.DeclaringType;

        if (Tracker.StaticCtorInvokeOrder.Contains(type)) return;

        // find and store static fields for later
        var declaredFields = AccessTools.GetDeclaredFields(type).Where(x => x.IsStatic && !x.IsLiteral);
        Tracker.StaticFields.AddRange(declaredFields);
    }

    public static void StaticCtorEnd()
    {
        var type = new StackFrame(1).GetMethod()?.DeclaringType;

        if (Tracker.StaticCtorInvokeOrder.Contains(type)) return;
        Patcher.Logger.LogDebug($"First static ctor invoke for {type?.FullName}");

        Tracker.StaticCtorInvokeOrder.Add(type);
    }
}