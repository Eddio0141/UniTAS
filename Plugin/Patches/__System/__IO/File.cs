using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using UniTASPlugin.FakeGameState.GameFileSystem;
using FileOrig = System.IO.File;
using DirectoryOrig = System.IO.Directory;

namespace UniTASPlugin.Patches.__System.__IO;

[HarmonyPatch]
static class File
{
    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.Exists))]
    class Exists
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref bool __result, string path)
        {
            __result = !string.IsNullOrEmpty(path) && path.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) < 0 && FileSystem.FileExists(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.Copy), new Type[] { typeof(string), typeof(string), typeof(bool) })]
    class Copy
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string sourceFileName, string destFileName, bool overwrite)
        {
            FileSystem.OsHelpers.Copy(Path.GetFullPath(sourceFileName), Path.GetFullPath(destFileName), overwrite);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), "InternalCopy")]
    class InternalCopy
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref string __result, string sourceFileName, string destFileName, bool overwrite)
        {
            string fullPathInternal = Path.GetFullPath(sourceFileName);
            string fullPathInternal2 = Path.GetFullPath(destFileName);
            FileSystem.OsHelpers.Copy(fullPathInternal, fullPathInternal2, overwrite);
            __result = fullPathInternal2;
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.Delete))]
    class Delete
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string path)
        {
            FileSystem.OsHelpers.DeleteFile(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.GetAccessControl), new Type[] { typeof(string), typeof(AccessControlSections) })]
    class GetAccessControl
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref FileSecurity __result)
        {
            __result = new FileSecurity();
            __result.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Allow));
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.SetAccessControl))]
    class SetAccessControl
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.GetAttributes))]
    class GetAttributes
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref FileAttributes __result, string path)
        {
            __result = FileSystem.OsHelpers.GetFileAttributes(path) ?? FileAttributes.Normal;
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.SetAttributes))]
    class SetAttributes
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string path, FileAttributes fileAttributes)
        {
            FileSystem.OsHelpers.SetFileAttributes(path, fileAttributes);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.GetCreationTime))]
    class GetCreationTime
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref DateTime __result, string path)
        {
            __result = FileSystem.OsHelpers.FileCreationTime(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.Move))]
    class Move
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string sourceFileName, string destFileName)
        {
            if (sourceFileName == null)
            {
                throw new ArgumentNullException("sourceFileName");
            }
            if (destFileName == null)
            {
                throw new ArgumentNullException("destFileName");
            }
            if (sourceFileName.Length == 0)
            {
                throw new ArgumentException("An empty file name is not valid.", "sourceFileName");
            }
            if (sourceFileName.Trim().Length == 0 || sourceFileName.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
            {
                throw new ArgumentException("The file name is not valid.");
            }
            if (destFileName.Length == 0)
            {
                throw new ArgumentException("An empty file name is not valid.", "destFileName");
            }
            if (destFileName.Trim().Length == 0 || destFileName.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
            {
                throw new ArgumentException("The file name is not valid.");
            }
            if (!FileSystem.OsHelpers.FileExists(sourceFileName))
            {
                throw new FileNotFoundException($"{sourceFileName} does not exist", sourceFileName);
            }
            string directoryName = Path.GetDirectoryName(destFileName);
            if (directoryName != string.Empty && !DirectoryOrig.Exists(directoryName))
            {
                throw new DirectoryNotFoundException("Could not find a part of the path.");
            }
            FileSystem.OsHelpers.MoveFile(sourceFileName, destFileName);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.Replace), new Type[] { typeof(string), typeof(string), typeof(string), typeof(bool) })]
    class Replace
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string sourceFileName, string destinationFileName, string destinationBackupFileName/*, bool ignoreMetadataErrors*/)
        {
            if (sourceFileName == null)
            {
                throw new ArgumentNullException("sourceFileName");
            }
            if (destinationFileName == null)
            {
                throw new ArgumentNullException("destinationFileName");
            }
            if (sourceFileName.Trim().Length == 0 || sourceFileName.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
            {
                throw new ArgumentException("sourceFileName");
            }
            if (destinationFileName.Trim().Length == 0 || destinationFileName.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
            {
                throw new ArgumentException("destinationFileName");
            }
            string fullPath = Path.GetFullPath(sourceFileName);
            string fullPath2 = Path.GetFullPath(destinationFileName);
            if (FileSystem.OsHelpers.DirectoryExists(fullPath))
            {
                throw new IOException($"{sourceFileName} is a directory");
            }
            if (FileSystem.OsHelpers.DirectoryExists(fullPath2))
            {
                throw new IOException($"{destinationFileName} is a directory");
            }
            if (!FileOrig.Exists(fullPath))
            {
                throw new FileNotFoundException($"{sourceFileName} does not exist", sourceFileName);
            }
            if (!FileOrig.Exists(fullPath2))
            {
                throw new FileNotFoundException($"{destinationFileName} does not exist", destinationFileName);
            }
            if (fullPath == fullPath2)
            {
                throw new IOException("Source and destination arguments are the same file.");
            }
            string text = null;
            if (destinationBackupFileName != null)
            {
                if (destinationBackupFileName.Trim().Length == 0 || destinationBackupFileName.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
                {
                    throw new ArgumentException("destinationBackupFileName");
                }
                text = Path.GetFullPath(destinationBackupFileName);
                if (FileSystem.OsHelpers.DirectoryExists(text))
                {
                    throw new IOException($"{destinationBackupFileName} is a directory");
                }
                if (fullPath == text)
                {
                    throw new IOException("Source and backup arguments are the same file.");
                }
                if (fullPath2 == text)
                {
                    throw new IOException("Destination and backup arguments are the same file.");
                }
            }
            if ((FileOrig.GetAttributes(fullPath2) & FileAttributes.ReadOnly) != 0)
            {
                throw new IOException("Destination file is read-only.");
            }
            FileSystem.OsHelpers.ReplaceFile(fullPath, fullPath2);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.GetLastAccessTime))]
    class GetLastAccessTime
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        public static bool Prefix(ref DateTime __result, string path)
        {
            __result = FileSystem.OsHelpers.FileAccessTime(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.GetLastWriteTime))]
    class GetLastWriteTime
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref DateTime __result, string path)
        {
            __result = FileSystem.OsHelpers.FileWriteTime(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.SetCreationTime))]
    class SetCreationTime
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string path, DateTime creationTime)
        {
            FileSystem.OsHelpers.SetFileCreationTime(path, creationTime);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.SetLastAccessTime))]
    class SetLastAccessTime
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string path, DateTime lastAccessTime)
        {
            FileSystem.OsHelpers.SetFileAccessTime(path, lastAccessTime);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.SetLastWriteTime))]
    class SetLastWriteTime
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string path, DateTime lastWriteTime)
        {
            FileSystem.OsHelpers.SetFileWriteTime(path, lastWriteTime);
            return false;
        }
    }
}