using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using UniTAS.Patcher.Extensions;

namespace UniTAS.Patcher;

public static class PatcherUtils
{
    public static string ProjectAssembly { get; } = typeof(Entry).Namespace;

    private static readonly string[] AssemblyExclusionsRaw =
    {
        "UnityEngine.*",
        "UnityEngine",
        "Unity.*",
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
        "Newtonsoft.Json"
    };

    private static readonly string[] AssemblyIncludeRaw =
    {
        "Unity.InputSystem",
        "UnityEngine.InputModule"
    };

    public static IEnumerable<string> AllTargetDllsWithGenericExclusions =>
        Directory.GetFiles(Paths.ManagedPath, "*.dll", SearchOption.TopDirectoryOnly)
            .Where(x =>
            {
                var fileWithoutExtension = Path.GetFileNameWithoutExtension(x);
                return fileWithoutExtension == null ||
                       AssemblyIncludeRaw.Any(a => fileWithoutExtension.Like(a)) ||
                       !AssemblyExclusionsRaw.Any(a => fileWithoutExtension.Like(a));
            })
            // isolate the filename
            .Select(Path.GetFileName);
}