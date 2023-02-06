namespace UniTASPlugin.GameEnvironment.InnerState.FileSystem;

public enum FileType
{
    Unknown = 0,
    Disk = 1,
    Char = 2,
    Pipe = 3,
    Remote = 32768, // 0x00008000
}