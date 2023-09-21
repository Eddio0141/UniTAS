using System.IO;
using BepInEx;

namespace UniTAS.Patcher.Utils;

public static class BepInExUtils
{
    public static void GenerateMissingDirs()
    {
        var dirs = new[]
        {
            Paths.PluginPath
        };

        foreach (var dir in dirs)
        {
            if (Directory.Exists(dir)) continue;

            StaticLogger.Log.LogInfo($"Creating missing BepInEx directory {dir}");
            Directory.CreateDirectory(dir);
        }
    }
}