using System;
using System.IO;

namespace UniTASPlugin.GameEnvironment.InnerState.FileSystem;

public interface IFileSystemManager
{
    // TODO error reporting
    void CreateDirectory(string path);
    void DeleteDirectory(string path);
    string[] GetFileSystemEntries(string path, string pathWithPattern, int attrs, int mask);
    string CurrentDirectory { get; set; }

    void MoveFile(string sourceFileName, string destFileName);
    void CopyFile(string sourceFileName, string destFileName, bool overwrite);
    void DeleteFile(string path);

    void ReplaceFile(string sourceFileName, string destinationFileName, string destinationBackupFileName,
        bool ignoreMetadataErrors);

    FileAttributes GetFileAttributes(string path);
    void SetFileAttributes(string path, FileAttributes fileAttributes);

    FileType GetFileType(IntPtr handle);
    FileStat GetFileStat(string path);

    IntPtr Open(string path, FileMode mode, FileAccess access, FileShare share, FileOptions options);
    void Close(IntPtr handle);

    int Read(IntPtr handle, byte[] dest, int destOffset, int count);
    int Write(IntPtr handle, in byte[] src, int srcOffset, int count);

    long Seek(IntPtr handle, long offset, SeekOrigin origin);
    // void Flush(IntPtr handle);

    long GetLength(IntPtr handle);
    void SetLength(IntPtr handle, long length);

    void SetFileTime(IntPtr handle, long creationTime, long lastAccessTime, long lastWriteTime);

    // TODO: implement
    // void Lock(IntPtr handle, long position, long length);
    // void Unlock(IntPtr handle, long position, long length);

    IntPtr ConsoleOutput { get; }
    IntPtr ConsoleInput { get; }
    IntPtr ConsoleError { get; }

    // TODO implement
    // void CreatePipe(out IntPtr readPipe, out IntPtr writePipe);
    // TODO implement
    // void DuplicateHandle(IntPtr sourceProcessHandle, IntPtr sourceHandle, IntPtr targetProcessHandle,
    //     out IntPtr targetHandle, int access, int inherit, int options);

    char VolumeSeparatorChar { get; }
    char DirectorySeparatorChar { get; }
    char AltDirectorySeparatorChar { get; }
    char PathSeparator { get; }

    void GetTempPath(out string path);
    void RemapPath(string path, out string newPath);
}