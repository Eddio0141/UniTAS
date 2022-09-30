using System;
using System.Collections.Generic;
using System.IO;

namespace UniTASPlugin.FakeGameState.GameFileSystem;

public static class FileSystem
{
    public static Dir Root { get; private set; }
    public static DeviceType DeviceType { get; }

    public static void Init(DeviceType device)
    {
        switch (device)
        {
            case DeviceType.Windows:
                {
                    Root = new Dir("C:", null);

                    // create path to where unity game is installed
                    var gameDir = Helper.GameRootDir();
                    var containingDir = new List<string>();

                    while (true)
                    {
                        var parentDir = Directory.GetParent(gameDir);
                        if (parentDir == null)
                            break;
                        gameDir = parentDir.FullName;
                        containingDir.Add(Path.GetDirectoryName(gameDir));
                    }
                    if (containingDir.Count > 0)
                        containingDir.RemoveAt(containingDir.Count - 1);
                    containingDir.Reverse();
                    var currentRoot = Root;
                    foreach (var dir in containingDir)
                    {
                        currentRoot = currentRoot.AddDir(dir);
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
