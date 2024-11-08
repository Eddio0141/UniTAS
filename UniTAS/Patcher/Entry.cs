using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using BepInEx;
using JetBrains.Annotations;
using Mono.Cecil;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Implementations;
using UniTAS.Patcher.ManualServices;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class Entry
{
    static Entry()
    {
        var assemblyExclusions = new HashSet<string>
        {
            // c# related
            "System.*",
            "System",
            "netstandard",
            "mscorlib",
            "Mono.*",
            "Mono",
            // no need
            "Newtonsoft.Json",

            // should be fine
            "UnityEngine.IMGUIModule",

            "Unity.InputSystem",
            "Unity.InputSystem.ForUI",
            "UnityEngine.InputModule",

            // ignore rewired, there's a fix for it
            "Rewired_*"
        };

        TargetDLLs = Directory.GetFiles(Paths.ManagedPath, "*.dll", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName).Where(
                x =>
                {
                    var fileWithoutExtension = Path.GetFileNameWithoutExtension(x);
                    return fileWithoutExtension == null || assemblyExclusions.Contains(fileWithoutExtension) ||
                           !assemblyExclusions.Any(a => fileWithoutExtension.Like(a));
                });
    }

    // List of assemblies to patch
    [UsedImplicitly] public static IEnumerable<string> TargetDLLs { get; }

    private static readonly PreloadPatcherProcessor PreloadPatcherProcessor = new();

    // Called before the assemblies are patched
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Initialize()
    {
        LoggingUtils.Init();
        StaticLogger.Log.LogInfo("Initializing UniTAS");

        BepInExUtils.GenerateMissingDirs();

        if (UniTASSha256Info.InvalidCache)
        {
            // cached assemblies are invalid as UniTAS has changed
            // .sha256 files
            foreach (var file in Directory.GetFiles(UniTASPaths.AssemblyCache, "*.sha256"))
            {
                File.Delete(file);
            }
        }

        StaticLogger.Log.LogInfo($"Found {PreloadPatcherProcessor.PreloadPatchers.Length} preload patchers");
        StaticLogger.Log.LogInfo($"Target dlls: {string.Join(", ", TargetDLLs.ToArray())}");
    }

    // Patches the assemblies
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Patch(ref AssemblyDefinition assembly)
    {
        var assemblyNameWithDll = $"{assembly.Name.Name}.dll";
        StaticLogger.Log.LogDebug($"Received assembly {assemblyNameWithDll} for patching");

        using var _ = Bench.Measure();

        // check cache validity and patch if needed
        if (TargetAssemblyCacheIsValid(ref assembly))
        {
            StaticLogger.Log.LogDebug("Skipping patching as it is cached");
            return;
        }

        foreach (var patcher in PreloadPatcherProcessor.PreloadPatchers)
        {
            StaticLogger.Log.LogInfo($"Patching {assemblyNameWithDll} with {patcher.GetType().Name}");

            var bench = Bench.Measure();
            patcher.Patch(ref assembly);
            bench.Dispose();
        }

        // save patched assembly to cache
        SavePatchedAssemblyToCache(assembly);
    }

    private static void SavePatchedAssemblyToCache(AssemblyDefinition assembly)
    {
        var dllCachePath = Path.Combine(UniTASPaths.AssemblyCache, $"{assembly.Name.Name}.dll");
        assembly.Write(dllCachePath);
    }

    private static bool TargetAssemblyCacheIsValid(ref AssemblyDefinition assembly)
    {
        var targetDllPath = assembly.MainModule.FileName;
        var assemblyNameWithDll = $"{assembly.Name.Name}.dll";

        using var targetDllOriginal = File.OpenRead(targetDllPath);
        using var dllSha256 = SHA256.Create();
        var hash = dllSha256.ComputeHash(targetDllOriginal);

        var cachedDllSha256Path = Path.Combine(UniTASPaths.AssemblyCache, $"{assemblyNameWithDll}.sha256");
        if (!File.Exists(cachedDllSha256Path))
        {
            File.WriteAllBytes(cachedDllSha256Path, hash);
            return false;
        }

        var cachedHash = File.ReadAllBytes(cachedDllSha256Path);
        if (!hash.SequenceEqual(cachedHash))
        {
            File.WriteAllBytes(cachedDllSha256Path, hash);
            return false;
        }

        var cachedDllPath = Path.Combine(UniTASPaths.AssemblyCache, assemblyNameWithDll);
        if (!File.Exists(cachedDllPath))
        {
            return false;
        }

        assembly = AssemblyDefinition.ReadAssembly(cachedDllPath);

        return true;
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Finish()
    {
        StaticLogger.Log.LogInfo("Finished preload patcher!");
        ContainerStarter.Init(RegisterTiming.Entry);
    }
}