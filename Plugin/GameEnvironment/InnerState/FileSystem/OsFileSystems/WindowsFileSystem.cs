namespace UniTASPlugin.GameEnvironment.InnerState.FileSystem.OsFileSystems;

public class WindowsFileSystem : FileSystem
{
    public override char DirectorySeparatorChar => '\\';
    public override char AltDirectorySeparatorChar => '/';
    public override char PathSeparator => ';';

    public override char[] PathSeparatorChars => new[]
    {
        DirectorySeparatorChar,
        AltDirectorySeparatorChar,
        VolumeSeparatorChar
    };

    public override string DirectorySeparatorStr => DirectorySeparatorChar.ToString();
    public override char VolumeSeparatorChar => ':';

    public override char[] InvalidPathChars => new[]
    {
        '"', '<', '>', '|', char.MinValue, '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\a', '\b', '\t',
        '\n', '\v', '\f', '\r', '\u000E', '\u000F', '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015',
        '\u0016', '\u0017', '\u0018', '\u0019', '\u001A', '\u001B', '\u001C', '\u001D', '\u001E', '\u001F'
    };

    public override bool DirEqualsVolume => DirectorySeparatorChar == VolumeSeparatorChar;

    public WindowsFileSystem(Dir root) : base(root)
    {
    }

    protected override bool PathIsAbsolute(string path)
    {
        return path.Contains(":");
    }

    protected override string GetDirectoryName(string path)
    {
        throw new System.NotImplementedException();
    }

    protected override string GetFileName(string path)
    {
        throw new System.NotImplementedException();
    }
}