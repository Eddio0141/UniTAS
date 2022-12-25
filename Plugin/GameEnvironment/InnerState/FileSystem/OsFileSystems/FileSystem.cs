using System.Linq;

namespace UniTASPlugin.GameEnvironment.InnerState.FileSystem.OsFileSystems;

public abstract class FileSystem
{
    private readonly Dir _root;
    public Dir CurrentDir { get; set; }

    protected FileSystem(Dir root)
    {
        _root = root;
        CurrentDir = _root;
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

    public Dir CreateDir(string path)
    {
        path = path.Trim();
        var dirs = path.Split(DirectorySeparatorChar, AltDirectorySeparatorChar).ToList();
        if (dirs.Count == 0) return null;

        var currentRoot = PathIsAbsolute(path) ? _root.AddDir(dirs[0]) : GetDir(dirs[0]);
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

        var dir = PathIsAbsolute(path) ? _root : CurrentDir;

        foreach (var d in dirs)
        {
            dir = dir.GetDir(d);
            if (dir == null)
                return null;
        }

        return dir;
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

    protected abstract bool PathIsAbsolute(string path);
    protected abstract string GetDirectoryName(string path);
    protected abstract string GetFileName(string path);
}