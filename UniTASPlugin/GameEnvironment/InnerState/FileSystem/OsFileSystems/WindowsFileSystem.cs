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

    public WindowsFileSystem() : base(new())
    {
        var cDrive = Root.AddDir("C:");
        var windows = cDrive.AddDir("Windows");
        var temp = windows.AddDir("Temp");

        InitDirs(temp);
    }

    public override bool PathIsAbsolute(string path)
    {
        return path.Contains(":");
    }

    public override string GetDirectoryName(string path)
    {
        var lastSlash = path.LastIndexOfAny(PathSeparatorChars);
        return lastSlash == -1 ? path : path.Substring(0, lastSlash);
    }

    public override string GetFileName(string path)
    {
        var lastSlash = path.LastIndexOfAny(PathSeparatorChars);
        return lastSlash == -1 ? path : path.Substring(lastSlash + 1);
    }
}