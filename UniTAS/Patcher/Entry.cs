using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using BepInEx;
using Mono.Cecil;
using UniTAS.Patcher.Implementations;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class Entry
{
    // List of assemblies to patch
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static IEnumerable<string> TargetDLLs =>
        Directory.GetFiles(Paths.ManagedPath, "*.dll", SearchOption.TopDirectoryOnly).Select(Path.GetFileName);

    private static readonly PreloadPatcherProcessor PreloadPatcherProcessor = new();

    // Called before the assemblies are patched
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Initialize()
    {
        StaticLogger.Log.LogInfo($"Found {PreloadPatcherProcessor.PreloadPatchers.Length} preload patchers");
        StaticLogger.Log.LogInfo(
            $"Target dlls: {string.Join(", ", TargetDLLs.ToArray())}");
    }

    // Patches the assemblies
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Patch(ref AssemblyDefinition assembly)
    {
        var assemblyNameWithDll = $"{assembly.Name.Name}.dll";
        StaticLogger.Log.LogDebug($"Received assembly {assemblyNameWithDll} for patching");

        foreach (var patcher in PreloadPatcherProcessor.PreloadPatchers)
        {
            // only patch the assembly if it's in the list of target assemblies
            if (!patcher.TargetDLLs.Contains(assemblyNameWithDll)) continue;
            StaticLogger.Log.LogInfo($"Patching {assemblyNameWithDll} with {patcher.GetType().Name}");
            patcher.Patch(ref assembly);
        }
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Finish()
    {
        StaticLogger.Log.LogInfo("Finished preload patcher!");
    }
}