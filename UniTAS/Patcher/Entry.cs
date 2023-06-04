using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Mono.Cecil;
using UniTAS.Patcher.Implementations;
using UniTAS.Patcher.Interfaces.Invoker;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class Entry
{
    // List of assemblies to patch
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static IEnumerable<string> TargetDLLs => PreloadPatcherProcessor.TargetDLLs;

    private static readonly PreloadPatcherProcessor PreloadPatcherProcessor = new();

    // Called before the assemblies are patched
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Initialize()
    {
        StaticLogger.Log.LogInfo($"Found {PreloadPatcherProcessor.PreloadPatchers.Length} preload patchers");
        StaticLogger.Log.LogDebug($"Target DLLs\n{string.Join("\n", PreloadPatcherProcessor.TargetDLLs)}");
    }

    // Patches the assemblies
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Patch(ref AssemblyDefinition assembly)
    {
        foreach (var patcher in PreloadPatcherProcessor.PreloadPatchers)
        {
            patcher.Patch(ref assembly);
        }
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Finish()
    {
        StaticLogger.Log.LogInfo("Finished preload patcher!");
        InvokeEventAttributes.Invoke<InvokeOnPatcherFinishAttribute>();
    }
}