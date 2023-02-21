using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Mono.Cecil;
using UniTAS.Patcher.PatcherUtils;

namespace UniTAS.Patcher;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class Patcher
{
    // List of assemblies to patch
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static IEnumerable<string> TargetDLLs { get; } = new[]
    {
        "UnityEngine.dll", "UnityEngine.CoreModule.dll"
    };

    // Patches the assemblies
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Patch(ref AssemblyDefinition assembly)
    {
        PatcherAttributeProcessor.Patch(ref assembly);
    }
}