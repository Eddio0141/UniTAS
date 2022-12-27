using System;
using System.IO;
using System.Text;

namespace UniTASPlugin.GameEnvironment.InnerState.FileSystem;

public class File : Entry
{
    public byte[] Data { get; set; }

    public string Text
    {
        get => Encoding.UTF8.GetString(Data);
        set => Data = Encoding.UTF8.GetBytes(value);
    }

    public File(string name, Dir parent = null, byte[] data = null) : base(name, parent, FileAttributes.Normal)
    {
        Data = data;
        CreationTime = DateTime.Now;
        if (data == null)
            Data = new byte[0];
    }

    public File(File file) : base(file)
    {
        Data = file.Data;
    }
}