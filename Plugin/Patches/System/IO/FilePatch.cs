using System;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using HarmonyLib;
using UniTASPlugin.LegacyFakeGameState.GameFileSystem;
using UniTASPlugin.ReverseInvoker;
using DirectoryOrig = System.IO.Directory;
using FileOrig = System.IO.File;
using PathOrig = System.IO.Path;
using DateTimeOrig = System.DateTime;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

namespace UniTASPlugin.Patches.System.IO;

[HarmonyPatch]
internal static class FilePatch
{
    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.Exists))]
    private class Exists
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref bool __result, string path)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            __result = !string.IsNullOrEmpty(path) &&
                       path.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) < 0 && FileSystem.FileExists(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.Copy), typeof(string), typeof(string), typeof(bool))]
    private class Copy
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(string sourceFileName, string destFileName, bool overwrite)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            FileSystem.OsHelpers.Copy(PathOrig.GetFullPath(sourceFileName), PathOrig.GetFullPath(destFileName),
                overwrite);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), "InternalCopy")]
    private class InternalCopy
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref string __result, string sourceFileName, string destFileName, bool overwrite)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            var fullPathInternal = PathOrig.GetFullPath(sourceFileName);
            var fullPathInternal2 = PathOrig.GetFullPath(destFileName);
            FileSystem.OsHelpers.Copy(fullPathInternal, fullPathInternal2, overwrite);
            __result = fullPathInternal2;
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.Delete))]
    private class Delete
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(string path)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            FileSystem.OsHelpers.DeleteFile(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.GetAccessControl), typeof(string), typeof(AccessControlSections))]
    private class GetAccessControl
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref FileSecurity __result)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            __result = new FileSecurity();
            __result.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl,
                AccessControlType.Allow));
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.SetAccessControl))]
    private class SetAccessControl
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix()
        {
            return Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.GetAttributes))]
    private class GetAttributes
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref FileAttributes __result, string path)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            __result = FileSystem.OsHelpers.GetFileAttributes(path) ?? FileAttributes.Normal;
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.SetAttributes))]
    private class SetAttributes
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(string path, FileAttributes fileAttributes)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            FileSystem.OsHelpers.SetFileAttributes(path, fileAttributes);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.GetCreationTime))]
    private class GetCreationTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref DateTimeOrig __result, string path)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            __result = FileSystem.OsHelpers.FileCreationTime(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.Move))]
    private class Move
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(string sourceFileName, string destFileName)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            if (sourceFileName == null)
            {
                throw new ArgumentNullException(nameof(sourceFileName));
            }

            if (destFileName == null)
            {
                throw new ArgumentNullException(nameof(destFileName));
            }

            if (sourceFileName.Length == 0)
            {
                throw new ArgumentException("An empty file name is not valid.", nameof(sourceFileName));
            }

            if (sourceFileName.Trim().Length == 0 ||
                sourceFileName.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
            {
                throw new ArgumentException("The file name is not valid.");
            }

            if (destFileName.Length == 0)
            {
                throw new ArgumentException("An empty file name is not valid.", nameof(destFileName));
            }

            if (destFileName.Trim().Length == 0 ||
                destFileName.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
            {
                throw new ArgumentException("The file name is not valid.");
            }

            if (!FileSystem.OsHelpers.FileExists(sourceFileName))
            {
                throw new FileNotFoundException($"{sourceFileName} does not exist", sourceFileName);
            }

            var directoryName = PathOrig.GetDirectoryName(destFileName);
            if (directoryName != string.Empty && !DirectoryOrig.Exists(directoryName))
            {
                throw new DirectoryNotFoundException("Could not find a part of the path.");
            }

            _ = FileSystem.OsHelpers.MoveFile(sourceFileName, destFileName);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.Replace), typeof(string), typeof(string), typeof(string),
        typeof(bool))]
    private class Replace
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(string sourceFileName, string destinationFileName,
            string destinationBackupFileName /*, bool ignoreMetadataErrors*/)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            if (sourceFileName == null)
            {
                throw new ArgumentNullException(nameof(sourceFileName));
            }

            if (destinationFileName == null)
            {
                throw new ArgumentNullException(nameof(destinationFileName));
            }

            if (sourceFileName.Trim().Length == 0 ||
                sourceFileName.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
            {
                throw new ArgumentException("sourceFileName");
            }

            if (destinationFileName.Trim().Length == 0 ||
                destinationFileName.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
            {
                throw new ArgumentException("destinationFileName");
            }

            var fullPath = PathOrig.GetFullPath(sourceFileName);
            var fullPath2 = PathOrig.GetFullPath(destinationFileName);
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

            if (destinationBackupFileName != null)
            {
                if (destinationBackupFileName.Trim().Length == 0 ||
                    destinationBackupFileName.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
                {
                    throw new ArgumentException("destinationBackupFileName");
                }

                var text = PathOrig.GetFullPath(destinationBackupFileName);
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
    private class GetLastAccessTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref DateTimeOrig __result, string path)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            __result = FileSystem.OsHelpers.FileAccessTime(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.GetLastWriteTime))]
    private class GetLastWriteTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref DateTimeOrig __result, string path)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            __result = FileSystem.OsHelpers.FileWriteTime(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.SetCreationTime))]
    private class SetCreationTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(string path, DateTimeOrig creationTime)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            FileSystem.OsHelpers.SetFileCreationTime(path, creationTime);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.SetLastAccessTime))]
    private class SetLastAccessTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(string path, DateTimeOrig lastAccessTime)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            FileSystem.OsHelpers.SetFileAccessTime(path, lastAccessTime);
            return false;
        }
    }

    [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.SetLastWriteTime))]
    private class SetLastWriteTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(string path, DateTimeOrig lastWriteTime)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking || PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            FileSystem.OsHelpers.SetFileWriteTime(path, lastWriteTime);
            return false;
        }
    }
}