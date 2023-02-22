using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Mono.Cecil;

namespace UniTAS.Patcher;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class Patcher
{
    // List of assemblies to patch
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static IEnumerable<string> TargetDLLs { get; } = new string[0];

    // Patches the assemblies
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Patch(ref AssemblyDefinition assembly)
    {
    }

    // Invoked after all assemblies are patched
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Finish()
    {
        var harmony = new HarmonyLib.Harmony("UniTAS.Patcher");
        harmony.PatchAll();
    }
}