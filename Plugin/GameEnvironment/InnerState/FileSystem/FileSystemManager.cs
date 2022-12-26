using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UniTASPlugin.GameEnvironment.InnerState.FileSystem.OsFileSystems;
using UniTASPlugin.GameInfo;
using UniTASPlugin.GameRestart;

namespace UniTASPlugin.GameEnvironment.InnerState.FileSystem;

/// <summary>
/// Manages virtual file systems of the game
/// Contains multiple instances of file systems
/// </summary>
public class FileSystemManager : IFileSystemManager, IOnGameRestart
{
    private OsFileSystems.FileSystem _windowsFileSystem;

    private readonly IGameInfo _gameInfo;
    private readonly IVirtualEnvironmentFactory _virtualEnvironmentFactory;

    public FileSystemManager(IGameInfo gameInfo, IVirtualEnvironmentFactory virtualEnvironmentFactory)
    {
        _gameInfo = gameInfo;
        _virtualEnvironmentFactory = virtualEnvironmentFactory;

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

    public void OnGameRestart()
    {
        var env = _virtualEnvironmentFactory.GetVirtualEnv();

        switch (env.Os)
        {
            case Os.Windows:
            {
#pragma warning disable CS0618
                var invalidPathCharsField = AccessTools.Field(typeof(Path), nameof(Path.InvalidPathChars));
#pragma warning restore CS0618
                invalidPathCharsField.SetValue(null, _windowsFileSystem.InvalidPathChars);
                var altDirectorySeparatorCharField =
                    AccessTools.Field(typeof(Path), nameof(Path.AltDirectorySeparatorChar));
                altDirectorySeparatorCharField.SetValue(null, _windowsFileSystem.AltDirectorySeparatorChar);
                var directorySeparatorCharField = AccessTools.Field(typeof(Path), nameof(Path.DirectorySeparatorChar));
                directorySeparatorCharField.SetValue(null, _windowsFileSystem.DirectorySeparatorChar);
                var pathSeparatorField = AccessTools.Field(typeof(Path), nameof(Path.PathSeparator));
                pathSeparatorField.SetValue(null, _windowsFileSystem.PathSeparator);
                var directorySeparatorStrField = AccessTools.Field(typeof(Path), "DirectorySeparatorStr");
                directorySeparatorStrField.SetValue(null, _windowsFileSystem.DirectorySeparatorStr);
                var volumeSeparatorCharField = AccessTools.Field(typeof(Path), nameof(Path.VolumeSeparatorChar));
                volumeSeparatorCharField.SetValue(null, _windowsFileSystem.VolumeSeparatorChar);
                var pathSeparatorCharsField = AccessTools.Field(typeof(Path), "PathSeparatorChars");
                pathSeparatorCharsField.SetValue(null, _windowsFileSystem.PathSeparatorChars);
                var dirEqualsVolumeField = AccessTools.Field(typeof(Path), "dirEqualsVolume");
                dirEqualsVolumeField.SetValue(null, _windowsFileSystem.DirEqualsVolume);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
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

    private static string PathToWindows(string path)
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