using HarmonyLib;
using System.IO;

namespace UniTASPlugin.VersionSafeWrapper;

public static class AppInfo
{
    static Traverse application()
    {
        return Traverse.CreateWithType("UnityEngine.Application");
    }

    public static string ProductName()
    {
        var productNameTraverse = application().Property("productName");
        if (productNameTraverse.PropertyExists())
            return productNameTraverse.GetValue<string>();

        // fallback, try get in c# way
        var crashHandlerExe = "UnityCrashHandler64.exe";
        var foundExe = "";
        var foundMultipleExe = false;
        var rootDir = Helper.GameRootDir();
        var rootFiles = Directory.GetFiles(rootDir);

        // iterate over exes in game root dir
        foreach (var path in rootFiles)
        {
            if (path == crashHandlerExe)
                continue;

            if (path.EndsWith(".exe"))
            {
                if (foundExe != "")
                {
                    foundMultipleExe = true;
                    break;
                }
                foundExe = path;
            }
        }

        if (foundExe == "" && !foundMultipleExe)
            throw new System.Exception("Could not find exe in game root dir");

        if (!foundMultipleExe)
            return Path.GetFileNameWithoutExtension(foundExe);

        // use game dir name and see if it matches exe
        var gameDirName = new DirectoryInfo(rootDir).Name;

        if (File.Exists(Path.Combine(rootDir, $"{gameDirName}.exe")))
            return gameDirName;

        return null;
    }
}
