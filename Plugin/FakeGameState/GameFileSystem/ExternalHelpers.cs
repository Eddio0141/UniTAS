using System;
using System.IO;

namespace UniTASPlugin.FakeGameState.GameFileSystem;

public static partial class FileSystem
{
    public static class ExternalHelpers
    {
        public static char DirectorySeparatorChar { get; private set; }
        public static char AltDirectorySeparatorChar { get; private set; }
        public static char PathSeparator { get; private set; }
        public static char[] PathSeparatorChars { get; private set; }
        public static string DirectorySeparatorStr { get; private set; }
        public static char VolumeSeparatorChar { get; private set; }
        public static char[] InvalidPathChars { get; private set; }
        public static bool dirEqualsVolume { get; private set; }

        public static void Init(DeviceType device)
        {
            switch (device)
            {
                case DeviceType.Windows:
                    {
                        DirectorySeparatorChar = '\\';
                        AltDirectorySeparatorChar = '/';
                        PathSeparator = ';';
                        VolumeSeparatorChar = ':';
                        InvalidPathChars = new[] { '<', '>', ':', '\\', '/', '|', '?', '*' };
                        DirectorySeparatorStr = DirectorySeparatorChar.ToString();
                        PathSeparatorChars = new[]
                        {
                        DirectorySeparatorChar,
                        AltDirectorySeparatorChar,
                        VolumeSeparatorChar
                    };
                        dirEqualsVolume = (DirectorySeparatorChar == VolumeSeparatorChar);
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        public static void FileStreamConstructorOpen(string path, FileMode mode, FileAccess access, FileShare share, FileOptions options)
        {
            
        }
    }
}