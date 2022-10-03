using System;
using System.IO;
using System.Linq;
using FileOrig = System.IO.File;

namespace UniTASPlugin.FakeGameState.GameFileSystem;

public static partial class FileSystem
{
    public static Dir Root { get; private set; }
    public static DeviceType DeviceType { get; }
    const ulong TOTAL_SIZE = 0x200000000; // 8gb

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
                    Plugin.Log.LogDebug($"writing test file to {gameDir}/test.txt");
                    FileOrig.AppendAllText(Path.Combine(gameDir, "test.txt"), "test");
                    Plugin.Log.LogDebug("done writing test file");
                    Plugin.Log.LogDebug($"test file exists: {FileOrig.Exists(Path.Combine(gameDir, "test.txt"))}");
                    break;
                }
            default:
                throw new NotImplementedException();
        }
    }

    public static Dir GetDir(string path)
    {
        var dir = Root;
        var dirs = path.Split(Path.DirectorySeparatorChar);
        foreach (var d in dirs)
        {
            Plugin.Log.LogDebug($"checking dir {d}");
            dir = dir.GetDir(d);
            Plugin.Log.LogDebug($"result {dir}");
            if (dir == null)
                return null;
        }
        return dir;
    }

    public static File GetFile(string path)
    {
        var dir = GetDir(Path.GetDirectoryName(path));
        if (dir == null)
            return null;
        return dir.GetFile(Path.GetFileName(path));
    }

    public static bool DirectoryExists(string path)
    {
        var foundDir = GetDir(path);
        return foundDir != null;
    }

    public static bool FileExists(string path)
    {
        var foundFile = GetFile(path);
        return foundFile != null;
    }

    public static void GetDiskFreeSpace(string path, out ulong availableFreeSpace, out ulong totalSize, out ulong totalFreeSpace)
    {
        if (path == null)
        {
            availableFreeSpace = 0;
            totalSize = 0;
            totalFreeSpace = 0;
            return;
        }
        switch (DeviceType)
        {
            case DeviceType.Windows:
                {
                    if (path == "C:" || path == "C:\\")
                    {
                        availableFreeSpace = TOTAL_SIZE;
                        totalSize = TOTAL_SIZE;
                        totalFreeSpace = TOTAL_SIZE;
                    }
                    else
                    {
                        availableFreeSpace = 0;
                        totalSize = 0;
                        totalFreeSpace = 0;
                    }
                    break;
                }
            default:
                throw new NotImplementedException();
        }
    }
}

public enum DeviceType
{
    Windows,
}
