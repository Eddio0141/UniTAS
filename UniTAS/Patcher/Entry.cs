using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using BepInEx;
using HarmonyLib;
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
                    return fileWithoutExtension == null || !(assemblyExclusions.Contains(fileWithoutExtension) ||
                                                             assemblyExclusions.Any(a => fileWithoutExtension.Like(a)));
                });
    }

    // List of assemblies to patch
    [UsedImplicitly] public static IEnumerable<string> TargetDLLs { get; }

    private static readonly PreloadPatcherProcessor PreloadPatcherProcessor = new();

    // Called before the assemblies are patched
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Initialize()
    {
        using var _ = Bench.Measure();

        LoggingUtils.Init();
        StaticLogger.Log.LogInfo("Initializing UniTAS");
        
        UniTASSha256Info.AssemblyLockFileValidate();
        UniTASSha256Info.AssemblyLockFileCreate();

        ThreadPool.QueueUserWorkItem(_ => { BepInExUtils.GenerateMissingDirs(); });

        if (UniTASSha256Info.UniTASInvalidCache)
        {
            AssemblyCacheFilled.Set();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                // cached assemblies are invalid as UniTAS has changed
                // .sha256 files
                foreach (var file in Directory.GetFiles(UniTASPaths.AssemblyCache, "*.sha256"))
                {
                    File.Delete(file);
                }

                DeletedCacheInfo.Set();
            });
        }
        else
        {
            DeletedCacheInfo.Set();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                // load all cached assemblies
                var paths = Directory.GetFiles(UniTASPaths.AssemblyCache, "*.sha256", SearchOption.TopDirectoryOnly);
                StaticLogger.LogDebug($"Found {paths.Length} cached assemblies");

                foreach (var hashPath in paths)
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        var dllPath = Path.Combine(Path.GetDirectoryName(hashPath)!,
                            Path.GetFileNameWithoutExtension(hashPath));
                        var originalDllPathFile = Path.Combine(UniTASPaths.AssemblyCache,
                            $"{Path.GetFileNameWithoutExtension(hashPath)}.path");
                        var originalDllPath =
                            File.Exists(originalDllPathFile) ? File.ReadAllText(originalDllPathFile) : null;
                        var actualDllPath = Path.Combine(Paths.ManagedPath, Path.GetFileName(dllPath));

                        if (!File.Exists(dllPath))
                        {
                            var writtenHash = File.Exists(actualDllPath);
                            if (writtenHash)
                            {
                                using var original = File.OpenRead(actualDllPath);
                                using var hashOriginal = SHA256.Create();
                                var writeHash = hashOriginal.ComputeHash(original);
                                File.WriteAllBytes(hashPath, writeHash);
                            }

                            AssemblyCache.TryAdd(Path.GetFileNameWithoutExtension(dllPath),
                                (null, writtenHash || originalDllPath == null ? null : hashPath));
                            AssemblyCacheFilled.Set();
                            return;
                        }

                        if (originalDllPath == null)
                        {
                            AssemblyCache.TryAdd(Path.GetFileNameWithoutExtension(dllPath), (null, hashPath));
                            AssemblyCacheFilled.Set();
                            return;
                        }

                        var hash = File.ReadAllBytes(hashPath);
                        var assembly = AssemblyDefinition.ReadAssembly(dllPath);

                        using var targetDllOriginal = File.OpenRead(originalDllPath);
                        using var dllSha256 = SHA256.Create();
                        var originalHash = dllSha256.ComputeHash(targetDllOriginal);

                        if (!hash.SequenceEqual(originalHash))
                        {
                            File.WriteAllBytes(hashPath, originalHash);

                            AssemblyCache.TryAdd(assembly.Name.Name, (null, null));
                            AssemblyCacheFilled.Set();

                            UniTASSha256Info.GameCacheInvalid = true;
                            return;
                        }

                        AssemblyCache.TryAdd(assembly.Name.Name, (assembly, null));
                        AssemblyCacheFilled.Set();
                    });
                }
            });
        }

        StaticLogger.Log.LogInfo($"Found {PreloadPatcherProcessor.PreloadPatchers.Length} preload patchers");
        StaticLogger.Log.LogInfo($"Target dlls: {TargetDLLs.Join()}");
    }

    private static readonly ManualResetEvent DeletedCacheInfo = new(false);

    // Patches the assemblies
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Patch(ref AssemblyDefinition assembly)
    {
        var assemblyNameWithDll = $"{assembly.Name.Name}.dll";
        StaticLogger.Log.LogDebug($"Received assembly {assemblyNameWithDll} for patching");

        using var _ = Bench.Measure();

        // check cache validity and patch if needed
        if (TargetAssemblyCache(ref assembly))
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

        // this cannot be done in another thread, it causes issues writing
        var dllCachePath = Path.Combine(UniTASPaths.AssemblyCache, assemblyNameWithDll);
        DeletedCacheInfo.WaitOne();
        assembly.Write(dllCachePath);
    }

    private static readonly ConcurrentDictionary<string, (AssemblyDefinition def, string writeHashPath)>
        AssemblyCache = new();

    private static readonly ManualResetEvent AssemblyCacheFilled = new(false);

    private static bool TargetAssemblyCache(ref AssemblyDefinition assembly)
    {
        if (UniTASSha256Info.UniTASInvalidCache)
        {
            WriteHash(assembly, Path.Combine(UniTASPaths.AssemblyCache, $"{assembly.Name.Name}.dll.sha256"));
            WriteDllPath(assembly);
            return false;
        }

        while (!AssemblyCache.ContainsKey(assembly.Name.Name))
        {
            AssemblyCacheFilled.WaitOne();
        }

        var (def, writeHashPath2) = AssemblyCache[assembly.Name.Name];
        if (def == null)
        {
            if (writeHashPath2 != null)
            {
                WriteHash(assembly, writeHashPath2);
                WriteDllPath(assembly);
            }

            return false;
        }

        assembly = def;
        return true;

        void WriteDllPath(AssemblyDefinition asmDef)
        {
            var path = Path.Combine(UniTASPaths.AssemblyCache, $"{asmDef.Name.Name}.dll.path");
            File.WriteAllText(path, asmDef.MainModule.FileName);
        }

        void WriteHash(AssemblyDefinition asmDef, string hashPath)
        {
            using var original = File.OpenRead(asmDef.MainModule.FileName);
            using var hashOriginal = SHA256.Create();
            var hash = hashOriginal.ComputeHash(original);
            File.WriteAllBytes(hashPath, hash);
        }
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Finish()
    {
        StaticLogger.Log.LogInfo("Finished preload patcher!");
        UniTASSha256Info.AssemblyLockFileDelete();
        ContainerStarter.Init(RegisterTiming.Entry);
    }
}