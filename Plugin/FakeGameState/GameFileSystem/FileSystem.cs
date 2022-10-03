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
                    break;
                }
            default:
                throw new NotImplementedException();
        }

        // create path to where unity game is installed
        var gameDir = Helper.GameRootDir();
        CreateDir(gameDir);
        Plugin.Log.LogDebug($"Directory exists: {Directory.Exists(gameDir)}");
        Plugin.Log.LogDebug($"writing test file to {gameDir}/test.txt");
        FileOrig.AppendAllText(Path.Combine(gameDir, "test.txt"), "test");
        Plugin.Log.LogDebug("done writing test file");
        Plugin.Log.LogDebug($"test file exists: {FileOrig.Exists(Path.Combine(gameDir, "test.txt"))}");
    }

    public static void DeleteFile(string path)
    {
        var file = GetFile(path);
        if (file == null)
            return;
        var parent = file.Parent;
        if (parent == null)
            return;
        parent.DeleteFile(file);
    }

    public static Dir CreateDir(string path)
    {
        if (!path.StartsWith(Root.Name + Path.DirectorySeparatorChar))
            return null;
        var split = path.Split(Path.DirectorySeparatorChar);
        var currentRoot = Root;
        foreach (var dir in split)
        {
            currentRoot = currentRoot.AddDir(dir);
        }
        return currentRoot;
    }

    public static Dir GetDir(string path)
    {
        if (!path.StartsWith(Root.Name + Path.DirectorySeparatorChar))
            return null;

        var dir = Root;
        var dirs = path.Split(Path.DirectorySeparatorChar).ToList();
        if (dirs.Count > 0)
            dirs.RemoveAt(0);
        foreach (var d in dirs)
        {
            dir = dir.GetDir(d);
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
}

public enum DeviceType
{
    Windows,
}
