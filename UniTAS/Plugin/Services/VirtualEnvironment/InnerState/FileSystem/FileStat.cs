using System.IO;

namespace UniTAS.Plugin.Services.VirtualEnvironment.InnerState.FileSystem;

public class FileStat
{
    public string Name { get; }
    public FileAttributes Attributes { get; }
    public long Length { get; }
    public long CreationTime { get; }
    public long LastAccessTime { get; }
    public long LastWriteTime { get; }

    public FileStat(string name, FileAttributes attributes, long length, long creationTime, long lastAccessTime,
        long lastWriteTime)
    {
        Name = name;
        Attributes = attributes;
        Length = length;
        CreationTime = creationTime;
        LastAccessTime = lastAccessTime;
        LastWriteTime = lastWriteTime;
    }
}