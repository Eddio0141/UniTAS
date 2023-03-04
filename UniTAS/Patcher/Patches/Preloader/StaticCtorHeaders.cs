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

namespace UniTAS.Patcher.Patches.Preloader;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class StaticCtorHeaders : PreloadPatcher
{
    private readonly string[] _assemblyExclusionsRaw =
    {
        "System.*",
        "System",
        "netstandard",
        "mscorlib",
        // "Mono.*",
        // "Mono",
        // "MonoMod.*",
        // "BepInEx.*",
        // "BepInEx",
        // "MonoMod.*",
        // "0Harmony",
        // "HarmonyXInterop",
        // "StructureMap",
        // "Antlr4.Runtime.Standard"
    };

    public override IEnumerable<string> TargetDLLs =>
        Directory.GetFiles(Paths.ManagedPath, "*.dll", SearchOption.TopDirectoryOnly)
            .Where(x =>
            {
                var fileWithoutExtension = Path.GetFileNameWithoutExtension(x);
                return fileWithoutExtension == null ||
                       !_assemblyExclusionsRaw.Any(a => a.Like(fileWithoutExtension));
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
                continue;
            }

            var patchMethod = typeof(PatchMethods).GetMethod(nameof(PatchMethods.TraceStack));
            var patchMethodRef = assembly.MainModule.ImportReference(patchMethod);

            var firstInstruction = staticCtor.Body.Instructions.First();
            var ilProcessor = staticCtor.Body.GetILProcessor();

            // insert call
            Trace.Write($"Inserting stacktrace call to static ctor of {type.FullName}");
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, patchMethodRef));
        }
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class PatchMethods
    {
        public static void TraceStack()
        {
            Trace.Write("Static ctor invoked");
            Trace.Write(new StackTrace());
        }
    }
}