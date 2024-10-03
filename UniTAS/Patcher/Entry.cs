using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Mono.Cecil;
using UniTAS.Patcher.Implementations;
using UniTAS.Patcher.ManualServices;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Utils;
#if TRACE
using System.Diagnostics;
#endif

namespace UniTAS.Patcher;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class Entry
{
    // List of assemblies to patch
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static IEnumerable<string> TargetDLLs => PreloadPatcherProcessor.TargetDLLs;

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
        StaticLogger.Log.LogInfo($"Target dlls: {string.Join(", ", PreloadPatcherProcessor.TargetDLLs.ToArray())}");
    }

    // Patches the assemblies
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Patch(ref AssemblyDefinition assembly)
    {
        var assemblyNameWithDll = $"{assembly.Name.Name}.dll";
        StaticLogger.Log.LogDebug($"Received assembly {assemblyNameWithDll} for patching");

#if TRACE
        var sw = new Stopwatch();
        sw.Start();
#endif

        // check cache validity and patch if needed
        if (TargetAssemblyCacheIsValid(ref assembly))
        {
            StaticLogger.Log.LogDebug("Skipping patching as it is cached");
#if TRACE
            sw.Stop();
            StaticLogger.Trace($"time elapsed modifying assembly: {sw.Elapsed.Milliseconds}ms");
#endif
            return;
        }

        foreach (var patcher in PreloadPatcherProcessor.PreloadPatchers)
        {
            // only patch the assembly if it's in the list of target assemblies
            if (!patcher.TargetDLLs.Contains(assemblyNameWithDll)) continue;
            StaticLogger.Log.LogInfo($"Patching {assemblyNameWithDll} with {patcher.GetType().Name}");
#if TRACE
            var swPatch = new Stopwatch();
            swPatch.Start();
#endif
            patcher.Patch(ref assembly);
#if TRACE
            swPatch.Stop();
            StaticLogger.Trace($"time elapsed processing patch: {swPatch.Elapsed.TotalMilliseconds}ms");
#endif
        }

#if TRACE
        sw.Stop();
        StaticLogger.Trace($"time elapsed modifying assembly: {sw.Elapsed.Milliseconds}ms");
#endif

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