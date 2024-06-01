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
    public static string Resources { get; } = Utility.CombinePaths(Paths.PatcherPluginPath, "UniTAS", "Resources");
    public static string Cache { get; } = Path.Combine(UniTASBase, "cache");
    public static string AssemblyCache { get; } = Path.Combine(Cache, "assemblies");
    public static string ConfigCache { get; } = Path.Combine(Cache, "config");
    public static string Config { get; } = Path.Combine(Paths.ConfigPath, CONFIG_FILE_NAME);
    public const string CONFIG_FILE_NAME = "UniTAS.cfg";
}