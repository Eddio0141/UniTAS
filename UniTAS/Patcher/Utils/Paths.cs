using System.IO;
using BepInEx;

namespace UniTAS.Patcher.Utils;

public static class UniTASPaths
{
    static UniTASPaths()
    {
        Directory.CreateDirectory(AssemblyCache);
        Directory.CreateDirectory(ConfigCache);
    }

    private static string UniTASBase { get; } = Path.Combine(Paths.GameRootPath, "UniTAS");
    public static string Benchmarks { get; } = Path.Combine(UniTASBase, "benchmarks");
    public static string Resources { get; } = Utility.CombinePaths(Paths.PatcherPluginPath, "UniTAS", "Resources");
    public static string Cache { get; } = Path.Combine(UniTASBase, "cache");
    public static string AssemblyCache { get; } = Path.Combine(Cache, "assemblies");
    public static string ConfigCache { get; } = Path.Combine(Cache, "config");
    public static string ConfigBepInEx { get; } = Path.Combine(Paths.ConfigPath, BepInExConfigFileName);
    public const string BepInExConfigFileName = "UniTAS.cfg";
    public static string ConfigBackend { get; } = Path.Combine(UniTASBase, BackendConfigFileName);
    private const string BackendConfigFileName = "save.dat";
}