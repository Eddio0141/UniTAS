using System;
using System.Collections.Generic;
using UniTASPlugin.GameEnvironment.InnerState.FileSystem.OsFileSystems;
using UniTASPlugin.GameInfo;

namespace UniTASPlugin.GameEnvironment.InnerState.FileSystem;

/// <summary>
/// Manages virtual file systems of the game
/// Contains multiple instances of file systems
/// </summary>
public class FileSystemManager
{
    private OsFileSystems.FileSystem _windowsFileSystem;

    private readonly IGameInfo _gameInfo;

    public FileSystemManager(IGameInfo gameInfo)
    {
        _gameInfo = gameInfo;

        Init();
    }

    public void Init()
    {
        var gameDirPath = _gameInfo.GameDirectory;

        var windowsPc = new Dir("PC");
        windowsPc.AddDir("C:");
        _windowsFileSystem = new WindowsFileSystem(windowsPc);
        var gameDirWindows = _windowsFileSystem.CreateDir(PathToWindows(gameDirPath));
        _windowsFileSystem.CurrentDir = gameDirWindows;
    }

    /// <summary>
    /// Determines the OS type of the path
    /// </summary>
    /// <param name="path"></param>
    /// <returns>If absolute and OS type</returns>
    /// <exception cref="Exception"></exception>
    private static KeyValuePair<bool, PlatformID> PathType(string path)
    {
        // absolute unix path
        if (path.StartsWith("/"))
        {
            return new(true, PlatformID.Unix);
        }

        // absolute windows path
        if (path.Contains(":"))
        {
            return new(true, PlatformID.Win32NT);
        }

        // relative unix path
        if (path.Contains("/"))
        {
            return new(false, PlatformID.Unix);
        }

        // relative windows path
        if (path.Contains("\\"))
        {
            return new(false, PlatformID.Win32NT);
        }

        throw new NotImplementedException($"Unknown path type, path: {path}");
    }

    private string PathToWindows(string path)
    {
        var pathType = PathType(path);

        if (pathType.Value == PlatformID.Win32NT)
        {
            return path;
        }

        switch (pathType.Value)
        {
            case PlatformID.Unix:
                return path.Replace("/", "\\");
            default:
                throw new NotImplementedException($"Unknown path type, path: {path}");
        }
    }
}