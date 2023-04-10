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

    public override IEnumerable<string> TargetDLLs => Utils.AllTargetDllsWithGenericExclusions;

    public override void Patch(ref AssemblyDefinition assembly)
    {
        var definition = assembly;
        if (_assemblyExclusionsRaw.Any(x => definition.Name.Name.Like(x))) return;

        // get all types in assembly that has static fields or a static ctor
        var types = assembly.Modules.SelectMany(m => m.GetAllTypes())
            .Where(t => t.HasFields && t.Fields.Any(f => f.IsStatic && !f.IsLiteral) ||
                        t.HasMethods && t.Methods.Any(m => m.IsStatic && m.IsConstructor));

        Trace.Write("Patching static ctors");
        foreach (var type in types)
        {
            // find static ctor
            var staticCtor = type.Methods.FirstOrDefault(m => m.IsConstructor && m.IsStatic);
            // add static ctor if not found
            if (staticCtor == null)
            {
                Trace.Write($"Adding static ctor to {type.FullName}");
                staticCtor = new(".cctor",
                    MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig
                    | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                    assembly.MainModule.ImportReference(typeof(void)));

                type.Methods.Add(staticCtor);
                var il = staticCtor.Body.GetILProcessor();
                il.Append(il.Create(OpCodes.Ret));
            }

            Trace.Write($"Patching static ctor of {type.FullName}");
            PatchStaticCtorHeader(assembly, staticCtor);
        }
    }

    /// <summary>
    /// Patch the start of the static ctor for our own purposes
    /// </summary>
    private static void PatchStaticCtorHeader(AssemblyDefinition assembly, MethodDefinition staticCtor)
    {
        var patchMethod = AccessTools.Method(typeof(PatchMethods), nameof(PatchMethods.StaticCtorHeader));
        var patchMethodRef = assembly.MainModule.ImportReference(patchMethod);

        var firstInstruction = staticCtor.Body.Instructions.First();
        var ilProcessor = staticCtor.Body.GetILProcessor();

        // insert call
        ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, patchMethodRef));
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

    public static void StaticCtorHeader()
    {
        var type = new StackFrame(1).GetMethod()?.DeclaringType;

        var declaredFields = AccessTools.GetDeclaredFields(type).Where(x => x.IsStatic && !x.IsLiteral);
        foreach (var field in declaredFields)
        {
            field.SetValue(null, null);
            // ResetFieldCount++;
        }

        if (Tracker.StaticCtorInvokeOrder.Contains(type)) return;
        Trace.Write($"First static ctor invoke for {type?.FullName}");

        Tracker.StaticCtorInvokeOrder.Add(type);
    }
}