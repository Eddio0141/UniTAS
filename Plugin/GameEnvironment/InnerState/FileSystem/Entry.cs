using System;
using System.IO;

namespace UniTASPlugin.GameEnvironment.InnerState.FileSystem;

public abstract class Entry
{
    public string Name { get; set; }
    public Dir Parent { get; internal set; }
    public DateTime CreationTime { get; set; }
    public DateTime AccessTime { get; set; }
    public DateTime WriteTime { get; set; }
    public FileAttributes Attributes { get; set; }

    public string Path => Parent == null ? (Name ?? "") : System.IO.Path.Combine(Parent.Path, Name);

    protected Entry(string name, Dir parent, FileAttributes attributes)
    {
        Name = name;
        Parent = parent;
        CreationTime = DateTime.Now;
        AccessTime = CreationTime;
        WriteTime = CreationTime;
        Attributes = attributes;
    }

    protected Entry(Entry entry)
    {
        Name = entry.Name;
        Parent = entry.Parent;
        CreationTime = entry.CreationTime;
        AccessTime = entry.AccessTime;
        WriteTime = entry.WriteTime;
        Attributes = entry.Attributes;
    }

    public void Delete()
    {
        Parent?.Delete(this);
    }
}