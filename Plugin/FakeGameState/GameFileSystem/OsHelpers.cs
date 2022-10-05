using System;
using System.Collections.Generic;
using System.IO;

namespace UniTASPlugin.FakeGameState.GameFileSystem;

public static partial class FileSystem
{
    class OpenHandle
    {
        public string Path { get; }
        public long Offset { get; set; }
        public FileOptions Options { get; }
        public FileAccess Access { get; }
        public FileShare Share { get; }
        public File File { get; }

        public OpenHandle(string path, long offset, FileOptions options, FileAccess access, FileShare share)
        {
            Path = path;
            Offset = offset;
            Options = options;
            Access = access;
            Share = share;
            File = GetFile(path);
        }

        ~OpenHandle()
        {
            if ((Options & FileOptions.DeleteOnClose) != 0)
                DeleteFile(Path);
        }

        public OpenHandle(string path, FileOptions options, FileAccess access, FileShare share) : this(path, 0, options, access, share) { }
    }

    public static class OsHelpers
    {
        static readonly Dictionary<string, OpenHandle> openHandles = new();

        public static void OpenFile(string path, FileMode mode, FileAccess access, FileShare share, FileOptions options)
        {
            Plugin.Log.LogDebug($"Opening file {path} with mode {mode}, access {access}, share {share}, options {options}");
            File file;
            OpenHandle handle = null;

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
                        dir.AddFile(filename);
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
                            file = CreateDir(Directory.GetParent(path).FullName).AddFile(Path.GetFileName(path));
                        handle = new OpenHandle(path, file.Data.Length, options, access, share);
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }

            if (mode != FileMode.Append)
                handle = new OpenHandle(path, options, access, share);

            openHandles.Add(path, handle);
        }

        public static void Close(string path)
        {
            openHandles.Remove(path);
        }

        public static long Seek(string path, long bufStart, SeekOrigin seekOrigin)
        {
            if (!openHandles.TryGetValue(path, out var handle))
                throw new IOException();

            switch (seekOrigin)
            {
                case SeekOrigin.Current:
                    bufStart += handle.Offset;
                    break;
                case SeekOrigin.End:
                    bufStart += handle.File.Data.Length;
                    break;
            }

            handle.Offset = bufStart;

            return bufStart;
        }

        public static long Length(string path)
        {
            if (!openHandles.TryGetValue(path, out var handle))
                throw new IOException();

            return handle.File.Data.Length;
        }

        public static int Write(string path, byte[] src, int src_offset, int count)
        {
            if (!openHandles.TryGetValue(path, out var handle))
                throw new IOException();

            var file = handle.File;
            var offset = handle.Offset;
            var data = file.Data;

            if (offset + count > data.Length)
            {
                var newData = new byte[offset + count];
                Array.Copy(data, newData, data.Length);
                file.Data = newData;
                data = newData;
            }

            Array.Copy(src, src_offset, data, offset, count);
            handle.Offset += count;

            return count;
        }

        public static void SetLength(string path, long length)
        {
            if (!openHandles.TryGetValue(path, out var handle))
                throw new IOException();

            var file = handle.File;
            var data = file.Data;

            if (length > data.Length)
            {
                var newData = new byte[length];
                Array.Copy(data, newData, data.Length);
                file.Data = newData;
            }
            else
            {
                var newData = new byte[length];
                Array.Copy(data, newData, length);
                file.Data = newData;
            }
        }

        public static int Read(string path, byte[] dest, int dest_offset, int count)
        {
            if (!openHandles.TryGetValue(path, out var handle))
                throw new IOException();

            var file = handle.File;
            var offset = handle.Offset;
            var data = file.Data;
            if (offset + count > data.Length)
                return -1;

            Array.Copy(data, offset, dest, dest_offset, count);
            handle.Offset += count;

            return count;
        }
    }
}
