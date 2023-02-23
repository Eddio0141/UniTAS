using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BepInEx.Logging;
using Mono.Cecil;
using UniTAS.Patcher.PreloadPatchUtils;

namespace UniTAS.Patcher;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class Patcher
{
    public static ManualLogSource Logger { get; } = new(typeof(Patcher).Namespace);

    // List of assemblies to patch
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static IEnumerable<string> TargetDLLs => PreloadPatcherProcessor.TargetDLLs;

    private static readonly PreloadPatcherProcessor PreloadPatcherProcessor = new();

    // Patches the assemblies
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Patch(ref AssemblyDefinition assembly)
    {
        Logger.LogInfo($"Patching {PreloadPatcherProcessor.PreloadPatchers.Length} assemblies");
        foreach (var patcher in PreloadPatcherProcessor.PreloadPatchers)
        {
            patcher.Patch(ref assembly);
        }
    }
}