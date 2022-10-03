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
                        InvalidPathChars = new[] {
                            '\u0022', '\u003C', '\u003E', '\u007C',
                            '\u0000', '\u0001', '\u0002', '\u0003',
                            '\u0004', '\u0005', '\u0006', '\u0007',
                            '\u0008', '\u0009', '\u000A', '\u000B',
                            '\u000C', '\u000D', '\u000E', '\u000F',
                            '\u0010', '\u0011', '\u0012', '\u0013',
                            '\u0014', '\u0015', '\u0016', '\u0017',
                            '\u0018', '\u0019', '\u001A', '\u001B',
                            '\u001C', '\u001D', '\u001E', '\u001F',
                        };
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

        public static void GetDiskFreeSpace(string path, out ulong availableFreeSpace, out ulong totalSize, out ulong totalFreeSpace)
        {
            if (path == null)
            {
                availableFreeSpace = 0;
                totalSize = 0;
                totalFreeSpace = 0;
                return;
            }
            switch (DeviceType)
            {
                case DeviceType.Windows:
                    {
                        if (path == "C:" || path == "C:\\")
                        {
                            availableFreeSpace = TOTAL_SIZE;
                            totalSize = TOTAL_SIZE;
                            totalFreeSpace = TOTAL_SIZE;
                        }
                        else
                        {
                            availableFreeSpace = 0;
                            totalSize = 0;
                            totalFreeSpace = 0;
                        }
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }
        }
    }
}