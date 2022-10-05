using System;
using System.IO;
using System.Text;

namespace UniTASPlugin.FakeGameState.GameFileSystem;

public class File : Entry
{
    public byte[] Data { get; set; }
    public string Text { get => Encoding.UTF8.GetString(Data); set => Encoding.UTF8.GetBytes(value); }
    public string Extension { get; }
    public FileAttributes Attributes { get; set; }

    public File(string name, Dir parent, byte[] data, FileAttributes attributes) : base(name, parent)
    {
        Data = data;
        Attributes = attributes;
        CreationTime = DateTime.Now;
    }

    public File(string name, Dir parent, byte[] data) : this(name, parent, data, FileAttributes.Normal) { }

    public File(string name, Dir parent, string data) : this(name, parent, Encoding.UTF8.GetBytes(data)) { }

    public File(string name, Dir parent) : this(name, parent, new byte[0]) { }
}
