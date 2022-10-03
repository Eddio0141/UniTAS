using System.Collections.Generic;

namespace UniTASPlugin.FakeGameState.GameFileSystem;

public class Dir : Entry
{
    readonly List<Entry> children;

    public Dir(string name, Dir parent)
    {
        Name = name;
        Parent = parent;
        children = new List<Entry>();
    }

    public void DeleteFile(File file)
    {
        children.Remove(file);
    }

    public Dir AddDir(string name)
    {
        var dir = GetDir(name);
        if (dir == null)
        {
            dir = new(name, this);
            children.Add(dir);
        }
        return dir;
    }

    public File AddFile(string name)
    {
        var file = GetFile(name);
        if (file == null)
        {
            file = new(name, this);
            children.Add(file);
        }
        return file;
    }

    public Dir GetDir(string name)
    {
        foreach (var child in children)
        {
            if (child is Dir dir && dir.Name == name)
                return dir;
        }
        return null;
    }

    public File GetFile(string name)
    {
        foreach (var child in children)
        {
            if (child is File file && file.Name == name)
                return file;
        }
        return null;
    }
}
