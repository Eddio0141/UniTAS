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

    public Dir AddDir(string name)
    {
        var addingDir = new Dir(name, this);
        if (!children.Contains(addingDir))
            children.Add(addingDir);
        return addingDir;
    }

    public File AddFile(string name)
    {
        var addingFile = new File(name, this);
        if (!children.Contains(addingFile))
            children.Add(addingFile);
        return addingFile;
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
}
