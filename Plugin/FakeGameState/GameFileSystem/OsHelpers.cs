using System;
using System.Collections.Generic;
using System.IO;

namespace UniTASPlugin.FakeGameState.GameFileSystem;

public static partial class FileSystem
{
    class OpenHandle
    {
        public string Path { get; }
        public ulong Offset { get; }
        public bool Read { get; }
        public bool Write { get; }
        bool deleteOnClose;

        public OpenHandle(string path, ulong offset, FileOptions options, FileAccess access, )
        {
            Path = path;
            Offset = offset;
            Read = read;
            Write = write;
        }

        ~OpenHandle()
        {
            if (deleteOnClose)
                DeleteFile(Path);
        }

        public OpenHandle(string path, bool read, bool write) : this(path, 0, read, write) { }
    }

    public static class OsHelpers
    {
        static int nextID = 0;
        static Dictionary<IntPtr, OpenHandle> openHandles = new();

        public static void OpenFile(string path, FileMode mode, FileAccess access, FileShare share, FileOptions options)
        {
            File file;
            OpenHandle handle = null;

            var read = access == FileAccess.Read || access == FileAccess.ReadWrite;
            var write = access == FileAccess.Write || access == FileAccess.ReadWrite;

            switch (mode)
            {
                case FileMode.CreateNew:
                    {
                        var check = GetFile(path);
                        if (check != null)
                            throw new IOException();
                        goto case FileMode.Create;
                    }
                case FileMode.Create:
                    {
                        var pathDir = Directory.GetParent(path);
                        var dir = CreateDir(pathDir.FullName);
                        var filename = Path.GetFileName(path);
                        file = dir.AddFile(filename);
                        break;
                    }
                case FileMode.Open:
                    {
                        file = GetFile(path);
                        if (file == null)
                            throw new FileNotFoundException();
                        break;
                    }
                case FileMode.OpenOrCreate:
                    {
                        file = GetFile(path);
                        if (file == null)
                            goto case FileMode.Create;
                        break;
                    }
                case FileMode.Truncate:
                    {
                        file = GetFile(path);
                        if (file == null)
                            throw new FileNotFoundException();
                        file.Data = new byte[0];
                        break;
                    }
                case FileMode.Append:
                    {
                        file = GetFile(path);
                        if (file == null)
                            throw new FileNotFoundException();
                        handle = new OpenHandle(path, (ulong)file.Data.Length, read, write);
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }

            if (mode != FileMode.Append)
                handle = new OpenHandle(path, read, write);

            if ((options & FileOptions.Asynchronous) != 0)
                throw new NotImplementedException();
            if ((options & FileOptions.DeleteOnClose) != 0)

                var handlePtr = new IntPtr(nextID++);
            openHandles.Add(handlePtr, handle);
        }
    }
}
