using System;
using HarmonyLib;
using Ninject;

namespace UniTASPlugin.VersionSafeWrapper;

public static class AppInfo
{
    private static readonly Traverse Application = Traverse.CreateWithType("UnityEngine.Application");
    private static readonly Traverse productName = Application.Property("productName");

    private static string productNameCache;

    public static string ProductName()
    {
        if (productName.PropertyExists())
            return productName.GetValue<string>();

        // fallback, try get in c# way
        var crashHandlerExe = "UnityCrashHandler64.exe";
        var foundExe = "";
        var foundMultipleExe = false;
        var rootDir = Helper.GameRootDir();
        var rev = Plugin.Instance.Kernel.Get<PatchReverseInvoker>();
        var rootFiles = rev.Invoke(System.IO.Directory.GetFiles, rootDir);

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
            throw new Exception("Could not find exe in game root dir");

        if (!foundMultipleExe)
        {
            productNameCache = rev.Invoke(System.IO.Path.GetFileNameWithoutExtension, foundExe);
            return productNameCache;
        }

        // use game dir name and see if it matches exe
        // TODO replace this instance creation
        var gameDirName = rev.Invoke(a => new System.IO.DirectoryInfo(a), rootDir).Name;

        if (rev.Invoke(System.IO.File.Exists, rev.Invoke(System.IO.Path.Combine, rootDir, $"{gameDirName}.exe")))
        {
            productNameCache = gameDirName;
            return gameDirName;
        }

        return null;
    }
}