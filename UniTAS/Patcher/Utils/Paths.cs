using System.IO;
using BepInEx;

namespace UniTAS.Patcher.Utils;

public static class UniTASPaths
{
    static UniTASPaths()
    {
        Directory.CreateDirectory(AssemblyCache);
    }

    private static string UniTASBase { get; } = Path.Combine(Paths.GameRootPath, "UniTAS");

    public static string Resources { get; } = Utility.CombinePaths(Paths.PatcherPluginPath, "UniTAS", "Resources");
    public static string Cache { get; } = Path.Combine(UniTASBase, "Cache");
    public static string AssemblyCache { get; } = Path.Combine(Cache, "Assemblies");
}