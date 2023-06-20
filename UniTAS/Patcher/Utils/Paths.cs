using BepInEx;

namespace UniTAS.Patcher.Utils;

public static class UniTASPaths
{
    public static string Resources { get; } = Utility.CombinePaths(Paths.PatcherPluginPath, "UniTAS", "Resources");
}