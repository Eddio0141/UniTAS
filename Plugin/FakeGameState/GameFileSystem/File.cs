using System.IO;
using System.Text;

namespace UniTASPlugin.FakeGameState.GameFileSystem;

public class File : Entry
{
    public byte[] Data { get; set; }
    public string Text { get => Encoding.UTF8.GetString(Data); set => Encoding.UTF8.GetBytes(value); }
    public string Extension { get; }
    public FileAttributes Attributes { get; set; }

    public File(string name, string extension, Dir parent, byte[] data, FileAttributes attributes)
    {
        Name = name;
        Parent = parent;
        Data = data;
        Extension = extension;
        Attributes = attributes;
    }

    public File(string name, string extension, Dir parent, byte[] data) : this(name, extension, parent, data, FileAttributes.Normal) { }

    public File(string name, string extension, Dir parent, string data) : this(name, extension, parent, Encoding.UTF8.GetBytes(data)) { }

    public File(string name, string extension, Dir parent) : this(name, extension, parent, new byte[0]) { }

    public File(string filename, Dir parent) : this(Path.GetFileName(filename), Path.GetExtension(filename), parent, new byte[0]) { }
}
