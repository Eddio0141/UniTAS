using System.Text;

namespace UniTASPlugin.FakeGameState.GameFileSystem;

public class File : Entry
{
    public byte[] Data { get; set; }
    public string Text { get => Encoding.UTF8.GetString(Data); set => Encoding.UTF8.GetBytes(value); }
    public string Extension { get; }

    public File(string name, string extension, Dir parent, byte[] data)
    {
        Name = name;
        Parent = parent;
        Data = data;
        Extension = extension;
    }

    public File(string name, string extension, Dir parent, string data) : this(name, extension, parent, Encoding.UTF8.GetBytes(data)) { }
}
