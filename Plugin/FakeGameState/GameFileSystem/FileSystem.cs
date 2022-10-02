using System;
using System.IO;
using System.Linq;

namespace UniTASPlugin.FakeGameState.GameFileSystem;

public static partial class FileSystem
{
    public static Dir Root { get; private set; }
    public static DeviceType DeviceType { get; }

    public static void Init(DeviceType device)
    {
        ExternalHelpers.Init(device);

        switch (device)
        {
            case DeviceType.Windows:
                {
                    Root = new Dir("C:", null);

                    // create path to where unity game is installed
                    var gameDir = Helper.GameRootDir();
                    var gameDirSplit = gameDir.Split(Path.DirectorySeparatorChar).ToList();
                    if (gameDirSplit.Count > 0)
                        gameDirSplit.RemoveAt(0);

                    var currentRoot = Root;
                    foreach (var dir in gameDirSplit)
                    {
                        Plugin.Log.LogDebug($"adding {dir}");
                        currentRoot = currentRoot.AddDir(dir);
                    }
                    Plugin.Log.LogDebug($"Directory exists: {Directory.Exists(gameDir)}");
                    break;
                }
            default:
                throw new NotImplementedException();
        }
    }

    public static bool DirectoryExists(string path)
    {
        var dir = Root;
        var dirs = path.Split(Path.DirectorySeparatorChar);
        foreach (var d in dirs)
        {
            dir = dir.GetDir(d);
            if (dir == null)
                return false;
        }

        return true;
    }
}

public enum DeviceType
{
    Windows,
}
