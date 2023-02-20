using System;
using System.IO;

namespace UniTAS.Plugin.GameEnvironment.InnerState.FileSystem;

public class FileHandle
{
    public File File { get; }
    public long Position { get; set; }
    public FileOptions Options { get; }
    public FileAccess Access { get; }
    public FileShare Share { get; }
    public IntPtr Handle { get; }

    public FileHandle(File file, long position, FileOptions options, FileAccess access, FileShare share, IntPtr handle)
    {
        File = file;
        Position = position;
        Options = options;
        Access = access;
        Share = share;
        Handle = handle;
    }

    public FileHandle(File file, FileOptions options, FileAccess access, FileShare share, IntPtr handle) : this(file, 0,
        options,
        access, share, handle)
    {
    }

    public FileHandle(File file, IntPtr handle) : this(file, 0, FileOptions.None, FileAccess.ReadWrite, FileShare.None,
        handle)
    {
    }

    ~FileHandle()
    {
        if ((Options & FileOptions.DeleteOnClose) != 0)
            File.Delete();
    }
}