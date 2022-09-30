using System.Collections.Generic;

namespace UniTASPlugin.FakeGameState.GameFileSystem;

public class Dir : Entry
{
    List<Entry> children;

    public Dir(string name, Dir parent)
    {
        Name = name;
        Parent = parent;
        children = new List<Entry>();
    }

    public Dir AddDir(string name)
    {
        var addingDir = new Dir(name, this);
        children.Add(addingDir);
        return addingDir;
    }
}
