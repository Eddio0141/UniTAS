using System;
using System.IO;

namespace UniTASPlugin.FakeGameState.GameFileSystem;

public abstract class Entry
{
    public string Name { get; set; }
    public Dir Parent { get; protected set; }
    public DateTime CreationTime { get; set; }
    public DateTime AccessTime { get; set; }
    public DateTime WriteTime { get; set; }

    public string FullName => Parent == null ? Name : Path.Combine(Parent.FullName, Name);

    protected Entry(string name, Dir parent)
    {
        Name = name;
        Parent = parent;
        CreationTime = DateTime.Now;
        AccessTime = CreationTime;
        WriteTime = CreationTime;
    }
}
