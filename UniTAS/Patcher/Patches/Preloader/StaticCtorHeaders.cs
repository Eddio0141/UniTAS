using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using BepInEx;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.PreloadPatchUtils;
using UniTAS.Patcher.Runtime;

namespace UniTAS.Patcher.Patches.Preloader;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class StaticCtorHeaders : PreloadPatcher
{
    private readonly string[] _assemblyExclusionsRaw =
    {
        // "UnityEngine.*",
        // "UnityEngine",
        // "Unity.*",
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

    public override IEnumerable<string> TargetDLLs =>
        Directory.GetFiles(Paths.ManagedPath, "*.dll", SearchOption.TopDirectoryOnly)
            .Where(x =>
            {
                var fileWithoutExtension = Path.GetFileNameWithoutExtension(x);
                return fileWithoutExtension == null ||
                       !_assemblyExclusionsRaw.Any(a => fileWithoutExtension.Like(a));
            })
            // isolate the filename
            .Select(Path.GetFileName);

    public override void Patch(ref AssemblyDefinition assembly)
    {
        // var typeExclusionsRaw = new[]
        // {
        //     // TODO remove this later
        //     // "UnityEngine.GUI"
        // };

        var types = assembly.Modules.SelectMany(m => m.Types)
            .Where(t => t.HasMethods && t.Methods.Any(m => m.IsConstructor && m.IsStatic));

        Trace.Write("Patching static ctors");
        foreach (var type in types)
        {
            // find static ctor
            var staticCtor = type.Methods.FirstOrDefault(m => m.IsConstructor && m.IsStatic);
            if (staticCtor == null)
            {
                Trace.Write("Already should've filtered out types without static ctors");
                return;
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
        var patchMethod = typeof(PatchMethods).GetMethod(nameof(PatchMethods.TraceStack));
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
    // TODO test if this works with generic types
    public static void TraceStack()
    {
        var type = new StackFrame(1).GetMethod()?.DeclaringType;
        Trace.Write($"Static ctor invoked for {type?.FullName}");

        if (Tracker.StaticCtorInvokeOrder.Contains(type)) return;

        Tracker.StaticCtorInvokeOrder.Add(type);
    }
}