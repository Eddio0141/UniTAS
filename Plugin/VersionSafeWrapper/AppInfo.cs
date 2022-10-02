using HarmonyLib;

namespace UniTASPlugin.VersionSafeWrapper;

public static class AppInfo
{
    static readonly Traverse Application = Traverse.CreateWithType("UnityEngine.Application");
    static readonly Traverse productName = Application.Property("productName");

    static string productNameCache = null;

    public static string ProductName()
    {
        if (productName.PropertyExists())
            return productName.GetValue<string>();

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
        {
            productNameCache = Path.GetFileNameWithoutExtension(foundExe);
            return productNameCache;
        }

        // use game dir name and see if it matches exe
        var gameDirName = new DirectoryInfo(rootDir).Name;

        if (File.Exists(Path.Combine(rootDir, $"{gameDirName}.exe")))
        {
            productNameCache = gameDirName;
            return gameDirName;
        }

        return null;
    }
}