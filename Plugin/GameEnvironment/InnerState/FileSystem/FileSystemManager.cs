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

    private OsFileSystems.FileSystem _currentFileSystem;

    public FileSystemManager(IGameInfo gameInfo, IVirtualEnvironmentFactory virtualEnvironmentFactory)
    {
        _gameInfo = gameInfo;
        _virtualEnvironmentFactory = virtualEnvironmentFactory;

        Init();
    }

    public void Init()
    {
        var gameDirPath = _gameInfo.GameDirectory;

        _windowsFileSystem = new WindowsFileSystem();
        var gameDirWindows = _windowsFileSystem.CreateDir(PathToWindows(gameDirPath));
        _windowsFileSystem.CurrentDir = gameDirWindows;

        var env = _virtualEnvironmentFactory.GetVirtualEnv();

        _currentFileSystem = (env.Os) switch
        {
            Os.Windows => _windowsFileSystem,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void OnGameRestart()
    {
        Init();

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

    private string PathToCurrentOS(string path)
    {
        var env = _virtualEnvironmentFactory.GetVirtualEnv();

        return env.Os switch
        {
            Os.Windows => PathToWindows(path),
            _ => throw new ArgumentOutOfRangeException()
        };
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

    public void CreateDirectory(string path)
    {
        path = PathToCurrentOS(path);
        _currentFileSystem.CreateDir(path);
    }

    public void DeleteDirectory(string path)
    {
        path = PathToCurrentOS(path);
        _currentFileSystem.DeleteDir(path);
    }

    public string[] GetFileSystemEntries(string path, string pathWithPattern, int attrs, int mask)
    {
        path = PathToCurrentOS(path);
        pathWithPattern = PathToCurrentOS(pathWithPattern);
        return _currentFileSystem.GetFileSystemEntries(path, pathWithPattern, (FileAttributes)attrs,
            (FileAttributes)mask);
    }

    public string CurrentDirectory
    {
        get => _currentFileSystem.CurrentDir.Path;
        set
        {
            var dir = _currentFileSystem.GetDir(value);
            if (dir == null) return;
            _currentFileSystem.CurrentDir = dir;
        }
    }

    public void MoveFile(string sourceFileName, string destFileName)
    {
        sourceFileName = PathToCurrentOS(sourceFileName);
        destFileName = PathToCurrentOS(destFileName);
        _currentFileSystem.MoveFile(sourceFileName, destFileName);
    }

    public void CopyFile(string sourceFileName, string destFileName, bool overwrite)
    {
        sourceFileName = PathToCurrentOS(sourceFileName);
        destFileName = PathToCurrentOS(destFileName);
        _currentFileSystem.CopyFile(sourceFileName, destFileName, overwrite);
    }

    public void DeleteFile(string path)
    {
        path = PathToCurrentOS(path);
        _currentFileSystem.DeleteFile(path);
    }

    public void ReplaceFile(string sourceFileName, string destinationFileName, string destinationBackupFileName,
        bool ignoreMetadataErrors)
    {
        sourceFileName = PathToCurrentOS(sourceFileName);
        destinationFileName = PathToCurrentOS(destinationFileName);
        destinationBackupFileName = PathToCurrentOS(destinationBackupFileName);
        _currentFileSystem.ReplaceFile(sourceFileName, destinationFileName, destinationBackupFileName,
            ignoreMetadataErrors);
    }

    public FileAttributes GetFileAttributes(string path)
    {
        path = PathToCurrentOS(path);
        return _currentFileSystem.GetAttributes(path);
    }

    public void SetFileAttributes(string path, FileAttributes fileAttributes)
    {
        path = PathToCurrentOS(path);
        _currentFileSystem.SetAttributes(path, fileAttributes);
    }

    public FileType GetFileType(IntPtr handle)
    {
        // TODO we only have a normal file type
        return FileType.Disk;
    }

    public FileStat GetFileStat(string path)
    {
        path = PathToCurrentOS(path);
        return _currentFileSystem.GetFileStat(path);
    }

    public IntPtr Open(string path, FileMode mode, FileAccess access, FileShare share, FileOptions options)
    {
        path = PathToCurrentOS(path);
        return _currentFileSystem.Open(path, mode, access, share, options);
    }

    public void Close(IntPtr handle)
    {
        _currentFileSystem.Close(handle);
    }

    public int Read(IntPtr handle, byte[] dest, int destOffset, int count)
    {
        return _currentFileSystem.Read(handle, dest, destOffset, count);
    }

    public int Write(IntPtr handle, in byte[] src, int srcOffset, int count)
    {
        return _currentFileSystem.Write(handle, src, srcOffset, count);
    }

    public long Seek(IntPtr handle, long offset, SeekOrigin origin)
    {
        return _currentFileSystem.Seek(handle, offset, origin);
    }

    public long GetLength(IntPtr handle)
    {
        return _currentFileSystem.GetLength(handle);
    }

    public void SetLength(IntPtr handle, long length)
    {
        _currentFileSystem.SetLength(handle, length);
    }

    public void SetFileTime(IntPtr handle, long creationTime, long lastAccessTime, long lastWriteTime)
    {
        _currentFileSystem.SetFileTime(handle, creationTime, lastAccessTime, lastWriteTime);
    }

    // public void Lock(IntPtr handle, long position, long length)
    // {
    // TODO
    //     // _currentFileSystem.Lock(handle, position, length);
    // }

    // public void Unlock(IntPtr handle, long position, long length)
    // {
    // TODO
    //     throw new NotImplementedException();
    // }

    public IntPtr ConsoleOutput => _currentFileSystem.ConsoleOutputHandle.Handle;
    public IntPtr ConsoleInput => _currentFileSystem.ConsoleInputHandle.Handle;
    public IntPtr ConsoleError => _currentFileSystem.ConsoleErrorHandle.Handle;

    // TODO
    // public void CreatePipe(out IntPtr readPipe, out IntPtr writePipe)
    // {
    //     throw new NotImplementedException();
    // }

    // TODO
    // public void DuplicateHandle(IntPtr sourceProcessHandle, IntPtr sourceHandle, IntPtr targetProcessHandle,
    //     out IntPtr targetHandle, int access, int inherit, int options)
    // {
    //     throw new NotImplementedException();
    // }

    public char VolumeSeparatorChar => _currentFileSystem.VolumeSeparatorChar;
    public char DirectorySeparatorChar => _currentFileSystem.DirectorySeparatorChar;
    public char AltDirectorySeparatorChar => _currentFileSystem.AltDirectorySeparatorChar;
    public char PathSeparator => _currentFileSystem.PathSeparator;

    public void GetTempPath(out string path)
    {
        path = _currentFileSystem.GetTempPath();
    }

    public void RemapPath(string path, out string newPath)
    {
        throw new NotImplementedException();
    }

    public void DumpHandles()
    {
        _currentFileSystem.DumpHandles();
    }
}