using System.Collections.Generic;
using System.IO;

namespace UniTASPlugin.GameEnvironment.InnerState.FileSystem;

public class Dir : Entry
{
    public List<Entry> Children { get; }

    public Dir(string name = null, Dir parent = null) : base(name, parent, FileAttributes.Directory)
    {
        Children = new();
    }

    public void Delete(Entry entry)
    {
        _ = Children.Remove(entry);
    }

    public Dir AddDir(string name)
    {
        var dir = GetDir(name);
        if (dir == null)
        {
            dir = new(name, this);
            Children.Add(dir);
        }

        return dir;
    }

    public File AddFile(string name)
    {
        var file = GetFile(name);
        if (file == null)
        {
            file = new(name, this);
            Children.Add(file);
        }

        return file;
    }

    public void AddEntry(Entry entry)
    {
        entry.Parent = this;
        Children.Add(entry);
    }

    public Dir GetDir(string name)
    {
        foreach (var child in Children)
        {
            if (child is Dir dir && dir.Name == name)
                return dir;
        }

        return null;
    }

    public File GetFile(string name)
    {
        foreach (var child in Children)
        {
            if (child is File file && file.Name == name)
                return file;
        }

        return null;
    }

    public File[] GetFilesRecursive()
    {
        var result = new List<File>();
        foreach (var child in Children)
        {
            if (child is File file)
                result.Add(file);
            else if (child is Dir dir)
                result.AddRange(dir.GetFilesRecursive());
        }

        return result.ToArray();
    }

    public Dir[] GetDirsRecursive()
    {
        var result = new List<Dir>();
        foreach (var child in Children)
        {
            if (child is Dir dir)
            {
                result.Add(dir);
                result.AddRange(dir.GetDirsRecursive());
            }
        }

        return result.ToArray();
    }
}