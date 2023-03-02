using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.PreloadPatchUtils;

namespace UniTAS.Patcher.Patches.Preloader;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class StaticCtorHeaders : PreloadPatcher
{
    public override IEnumerable<string> TargetDLLs => new[]
        { "UnityEngine.CoreModule.dll", "UnityEngine.dll", "Assembly-CSharp.dll" };

    public override void Patch(ref AssemblyDefinition assembly)
    {
        var assemblyExclusionsRaw = new[]
        {
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
            "StructureMap",
            "Antlr4.Runtime.Standard"
        };

        var typeExclusionsRaw = new[]
        {
            // TODO remove this later
            "UnityEngine.GUI"
        };

        foreach (var x in assemblyExclusionsRaw)
        {
            if (!assembly.Name.Name.Like(x)) continue;

            Trace.Write($"Skipping {assembly.Name.Name} due to exclusion");
            return;
        }

        var types = assembly.Modules.SelectMany(m => m.Types)
            .Where(t => !typeExclusionsRaw.Any(x =>
                t.FullName.Like(x) && t.HasMethods && t.Methods.Any(m => m.IsConstructor && m.IsStatic)));

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