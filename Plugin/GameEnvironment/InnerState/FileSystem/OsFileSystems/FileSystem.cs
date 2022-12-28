using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UniTASPlugin.GameEnvironment.InnerState.FileSystem.OsFileSystems;

public abstract class FileSystem
{
    protected readonly Dir Root;
    public Dir CurrentDir { get; set; }
    private Dir _tempDir;

    private readonly List<FileHandle> _handles = new();
    private int _nextHandle = ConsoleErrorHandleIndex + 1;

    private readonly File _consoleOutput = new("stdout");
    private readonly File _consoleInput = new("stdin");
    private readonly File _consoleError = new("stderr");

    private const int ConsoleOutputHandleIndex = 1;
    private const int ConsoleInputHandleIndex = 2;
    private const int ConsoleErrorHandleIndex = 3;

    public readonly FileHandle ConsoleOutputHandle;
    public readonly FileHandle ConsoleInputHandle;
    public readonly FileHandle ConsoleErrorHandle;

    protected FileSystem(Dir root)
    {
        Root = root;
        CurrentDir = Root;

        ConsoleOutputHandle = new(_consoleOutput, new(ConsoleOutputHandleIndex));
        ConsoleInputHandle = new(_consoleInput, new(ConsoleInputHandleIndex));
        ConsoleErrorHandle = new(_consoleError, new(ConsoleErrorHandleIndex));
    }

    protected void InitDirs(Dir tempDir)
    {
        _tempDir = tempDir;
    }

    public abstract char DirectorySeparatorChar { get; }
    public abstract char AltDirectorySeparatorChar { get; }
    public abstract char PathSeparator { get; }
    public abstract char[] PathSeparatorChars { get; }
    public abstract string DirectorySeparatorStr { get; }
    public abstract char VolumeSeparatorChar { get; }
    public abstract char[] InvalidPathChars { get; }
    public abstract bool DirEqualsVolume { get; }

    public void DeleteFile(string path)
    {
        var file = GetFile(path);
        file?.Parent?.Delete(file);
    }

    public void DeleteDir(string path)
    {
        var dir = GetDir(path);
        dir?.Parent?.Delete(dir);
    }

    public Dir CreateDir(string path)
    {
        path = path.Trim();
        var dirs = path.Split(DirectorySeparatorChar, AltDirectorySeparatorChar).ToList();
        if (dirs.Count == 0) return null;

        var currentRoot = PathIsAbsolute(path) ? Root.AddDir(dirs[0]) : GetDir(dirs[0]);
        if (currentRoot == null) return null;

        dirs.RemoveAt(0);

        foreach (var dir in dirs)
        {
            currentRoot = currentRoot.AddDir(dir);
        }

        return currentRoot;
    }

    public Dir GetDir(string path)
    {
        path = path.Trim();
        var dirs = path.Split(DirectorySeparatorChar, AltDirectorySeparatorChar).ToList();
        if (dirs.Count == 0) return null;

        var dir = PathIsAbsolute(path) ? Root : CurrentDir;

        foreach (var d in dirs)
        {
            dir = dir.GetDir(d);
            if (dir == null)
                return null;
        }

        return dir;
    }

    public void MoveFile(string sourcePath, string destPath)
    {
        var file = GetFile(sourcePath);
        if (file == null) return;

        var destDirPath = GetDirectoryName(destPath);
        var destDir = GetDir(destDirPath);

        file.Parent?.Delete(file);

        destDir ??= CreateDir(destDirPath);

        // TODO check if file already exists
        var destFileName = GetFileName(destPath);
        var foundDestFile = destDir.GetFile(destFileName);
        if (foundDestFile != null)
            destDir.Delete(foundDestFile);

        destDir.AddEntry(file);

        file.Name = destFileName;
    }

    public File GetFile(string path)
    {
        var dir = GetDir(GetDirectoryName(path));
        return dir?.GetFile(GetFileName(path));
    }

    public bool DirectoryExists(string path)
    {
        return GetDir(path) != null;
    }

    public bool FileExists(string path)
    {
        return GetFile(path) != null;
    }

    public string[] GetFileSystemEntries(string path, string pathWithPattern, FileAttributes attrs,
        FileAttributes mask)
    {
        var dir = GetDir(path);
        if (dir == null) return new string[0];

        // get paths
        var paths = dir.Children
            .Where(x => PathMatchesPattern(x.Path, pathWithPattern))
            .Where(x => (x.Attributes & mask) == attrs)
            .Select(x => x.Name)
            .ToArray();

        return paths;
    }

    private static bool PathMatchesPattern(string path, string pattern)
    {
        // * matches any number of characters
        // ? matches any single character

        var i = 0;
        var j = 0;
        while (i < path.Length && j < pattern.Length)
        {
            if (pattern[j] == '*')
            {
                if (j == pattern.Length - 1) return true;
                var next = pattern[j + 1];
                while (i < path.Length && path[i] != next) i++;
                j++;
            }
            else if (pattern[j] == '?' || path[i] == pattern[j])
            {
                i++;
                j++;
            }
            else
            {
                return false;
            }
        }

        return i == path.Length && j == pattern.Length;
    }

    public void CopyFile(string sourcePath, string destPath, bool overwrite)
    {
        var file = GetFile(sourcePath);
        if (file == null) return;

        var destDirPath = GetDirectoryName(destPath);
        var destDir = GetDir(destDirPath);

        destDir ??= CreateDir(destDirPath);

        // TODO check if file already exists
        var destFileName = GetFileName(destPath);
        var foundDestFile = destDir.GetFile(destFileName);
        if (foundDestFile != null)
        {
            if (overwrite)
                destDir.Delete(foundDestFile);
            else
                return;
        }

        var copiedFile = new File(file)
        {
            Name = destFileName
        };
        destDir.AddEntry(copiedFile);
    }

    // ReSharper disable once UnusedParameter.Global
    public void ReplaceFile(string sourceFile, string destFile, string destBackupName, bool ignoreMetadataErrors)
    {
        var file = GetFile(sourceFile);
        if (file == null) return;

        var destDirPath = GetDirectoryName(destFile);
        var destDir = GetDir(destDirPath);
        destDir ??= CreateDir(destDirPath);

        var destFileName = GetFileName(destFile);
        var foundDestFile = destDir.GetFile(destFileName);
        if (foundDestFile != null)
            destDir.Delete(foundDestFile);

        var copiedFile = new File(file)
        {
            Name = destFileName
        };
        destDir.AddEntry(copiedFile);

        if (destBackupName != null)
        {
            var backupDirPath = GetDirectoryName(destBackupName);
            var backupDir = GetDir(backupDirPath);
            backupDir ??= CreateDir(backupDirPath);

            var backupFileName = GetFileName(destBackupName);
            var foundBackupFile = backupDir.GetFile(backupFileName);
            if (foundBackupFile != null)
                backupDir.Delete(foundBackupFile);

            var backupFile = new File(file)
            {
                Name = backupFileName
            };
            backupDir.AddEntry(backupFile);
        }

        DeleteFile(sourceFile);

        // TODO use ignoreMetadataErrors
    }

    public FileAttributes GetAttributes(string path)
    {
        var file = GetFile(path);
        if (file != null)
            return file.Attributes;

        var dir = GetDir(path);

        return dir?.Attributes ?? 0;
    }

    public void SetAttributes(string path, FileAttributes attrs)
    {
        var file = GetFile(path);
        if (file != null)
        {
            file.Attributes = attrs;
            return;
        }

        var dir = GetDir(path);
        if (dir != null)
        {
            dir.Attributes = attrs;
        }
    }

    public FileStat GetFileStat(string path)
    {
        var file = GetFile(path);

        if (file == null)
            return null;

        return new(file.Path, file.Attributes, file.Data.Length, file.CreationTime.ToFileTime(),
            file.AccessTime.ToFileTime(),
            file.WriteTime.ToFileTime());
    }

    public IntPtr Open(string path, FileMode mode, FileAccess access, FileShare share, FileOptions options)
    {
        var file = GetFile(path);
        if (file == null)
        {
            if (mode == FileMode.Open)
                return IntPtr.Zero;

            var dirPath = GetDirectoryName(path);
            var dir = GetDir(dirPath);
            dir ??= CreateDir(dirPath);

            var fileName = GetFileName(path);
            file = new(fileName, dir);
            dir.AddEntry(file);
        }

        var handle = new FileHandle(file, options, access, share, new(_nextHandle));
        _handles.Add(handle);

        _nextHandle += 1;
        if (_nextHandle == -1)
            _nextHandle = ConsoleErrorHandleIndex;

        return handle.Handle;
    }

    public void Close(IntPtr handle)
    {
        var fileHandle = _handles.FirstOrDefault(x => x.Handle == handle);
        if (fileHandle == null)
            return;

        _handles.Remove(fileHandle);
    }

    public int Read(IntPtr handle, byte[] buffer, int offset, int count)
    {
        var fileHandle = _handles.FirstOrDefault(x => x.Handle == handle);

        var file = fileHandle?.File;
        if (file == null)
            return -1;

        var data = file.Data;
        var dataLength = data.Length;

        if (fileHandle.Position >= dataLength)
            return 0;

        var readCount = Math.Min(count, dataLength - fileHandle.Position);
        Array.Copy(data, fileHandle.Position, buffer, offset, readCount);
        fileHandle.Position += readCount;

        return (int)readCount;
    }

    public int Write(IntPtr handle, byte[] buffer, int offset, int count)
    {
        var fileHandle = _handles.FirstOrDefault(x => x.Handle == handle);

        var file = fileHandle?.File;
        if (file == null)
            return -1;

        // allocate more space if needed
        var data = file.Data;
        var dataLength = data.Length;

        if (fileHandle.Position + count > dataLength)
        {
            var newData = new byte[fileHandle.Position + count];
            Array.Copy(data, newData, dataLength);
            file.Data = newData;
            data = newData;
        }

        Array.Copy(buffer, offset, data, fileHandle.Position, count);
        fileHandle.Position += count;

        return count;
    }

    public long Seek(IntPtr handle, long offset, SeekOrigin origin)
    {
        var fileHandle = _handles.FirstOrDefault(x => x.Handle == handle);

        var file = fileHandle?.File;
        if (file == null)
            return -1;

        var data = file.Data;
        var dataLength = data.Length;

        var position = fileHandle.Position;

        switch (origin)
        {
            case SeekOrigin.Begin:
                position = offset;
                break;
            case SeekOrigin.Current:
                position += offset;
                break;
            case SeekOrigin.End:
                position = dataLength + offset;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
        }

        if (position < 0)
            position = 0;
        else if (position > dataLength)
            position = dataLength;

        fileHandle.Position = position;

        return position;
    }

    public long GetLength(IntPtr handle)
    {
        var fileHandle = _handles.FirstOrDefault(x => x.Handle == handle);

        var file = fileHandle?.File;
        if (file == null)
            return -1;

        return file.Data.Length;
    }

    public void SetLength(IntPtr handle, long length)
    {
        var fileHandle = _handles.FirstOrDefault(x => x.Handle == handle);

        var file = fileHandle?.File;
        if (file == null)
            return;

        var data = file.Data;
        var dataLength = data.Length;

        if (length == dataLength)
            return;

        var newData = new byte[length];
        Array.Copy(data, newData, length < dataLength ? length : dataLength);
        file.Data = newData;
    }

    public void SetFileTime(IntPtr handle, long creationTime, long lastAccessTime, long lastWriteTime)
    {
        var fileHandle = _handles.FirstOrDefault(x => x.Handle == handle);

        var file = fileHandle?.File;
        if (file == null)
            return;

        file.CreationTime = DateTime.FromFileTime(creationTime);
        file.AccessTime = DateTime.FromFileTime(lastAccessTime);
        file.WriteTime = DateTime.FromFileTime(lastWriteTime);
    }

    // public void Lock(IntPtr handle, long position, long length)
    // {
    // TODO
    // }

    public string GetTempPath()
    {
        return _tempDir.Path;
    }

    public void DumpHandles()
    {
        _handles.Clear();
    }

    public static string RemapPath(string path)
    {
        return path;
    }

    public abstract bool PathIsAbsolute(string path);
    public abstract string GetDirectoryName(string path);
    public abstract string GetFileName(string path);
}