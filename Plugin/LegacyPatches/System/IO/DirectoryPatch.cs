using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using HarmonyLib;
using UniTASPlugin.LegacyFakeGameState.GameFileSystem;
using UniTASPlugin.ReverseInvoker;
using DirOrig = System.IO.Directory;
using FileOrig = System.IO.File;
using PathOrig = System.IO.Path;
using DateTimeOrig = System.DateTime;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

namespace UniTASPlugin.LegacyPatches.System.IO;

[HarmonyPatch]
internal static class DirectoryPatch
{
    private static class Helper
    {
        private static readonly Traverse PathValidateTraverse =
            Traverse.Create(typeof(PathOrig)).Method("Validate", new[] { typeof(string) });

        private static readonly Traverse EnvironmentIsRunningOnWindowsTraverse =
            Traverse.Create(typeof(global::System.Environment)).Property("IsRunningOnWindows");

        public static void PathValidate(string path)
        {
            _ = PathValidateTraverse.GetValue(path);
        }

        public static bool EnvironmentIsRunningOnWindows()
        {
            return EnvironmentIsRunningOnWindowsTraverse.GetValue<bool>();
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.Exists))]
    private class DirExists
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(string path, ref bool __result)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking ||
                PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            __result = FileSystem.DirectoryExists(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), "InternalGetFileDirectoryNames")]
    private class InternalGetFileDirectoryNames
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref string[] __result, string path, string searchPattern, bool includeFiles,
            bool includeDirs, SearchOption searchOption)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking ||
                PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            __result = FileSystem.OsHelpers.GetPaths(path, searchPattern, includeFiles, includeDirs, searchOption);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), "EnumerateFileSystemNames")]
    private class EnumerateFileSystemNames
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref IEnumerable<string> __result, string path, string searchPattern,
            SearchOption searchOption, bool includeFiles, bool includeDirs)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking ||
                PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            __result = FileSystem.OsHelpers.GetPaths(path, searchPattern, includeFiles, includeDirs, searchOption);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.GetDirectoryRoot))]
    private class GetDirectoryRoot
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref string __result, string path)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking ||
                PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            Helper.PathValidate(path);
            __result = new(FileSystem.ExternalHelpers.DirectorySeparatorChar, 1);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.CreateDirectory), typeof(string))]
    private class CreateDirectory
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref DirectoryInfo __result, string path)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking ||
                PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.Length == 0)
            {
                throw new ArgumentException("Path is empty");
            }

            if (path.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
            {
                throw new ArgumentException("Path contains invalid chars");
            }

            if (path.Trim().Length == 0)
            {
                throw new ArgumentException("Only blank characters in path");
            }

            if (FileOrig.Exists(path))
            {
                throw new IOException("Cannot create " + path + " because a file with the same name already exists.");
            }

            if (Helper.EnvironmentIsRunningOnWindows() && path == ":")
            {
                throw new ArgumentException("Only ':' In path");
            }

            __result = Traverse.Create(typeof(DirOrig)).Method("CreateDirectoriesInternal")
                .GetValue<DirectoryInfo>(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), "CreateDirectoriesInternal")]
    private class CreateDirectoriesInternal
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref DirectoryInfo __result, string path)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking ||
                PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            var dirInfoConstructor =
                AccessTools.Constructor(typeof(DirectoryInfo), new[] { typeof(string), typeof(bool) });
            var directoryInfo = (DirectoryInfo)dirInfoConstructor.Invoke(null, new object[] { path, true });
            if (directoryInfo.Parent is { Exists: false })
            {
                directoryInfo.Parent.Create();
            }

            FileSystem.OsHelpers.CreateDir(path);
            __result = directoryInfo;
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.Delete), typeof(string))]
    private class Delete
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(string path)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking ||
                PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            Helper.PathValidate(path);
            if (Helper.EnvironmentIsRunningOnWindows() && path == ":")
            {
                throw new NotSupportedException("Only ':' In path");
            }

            if (FileOrig.Exists(path))
            {
                throw new IOException("Directory does not exist, but a file of the same name exists.");
            }

            /*
            if (MonoIO.ExistsSymlink(path, out monoIOError))
            {
                flag = MonoIO.DeleteFile(path, out monoIOError);
            }
            else
            */
            FileSystem.OsHelpers.DeleteEmptyDir(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), "RecursiveDelete")]
    private class RecursiveDelete
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(string path)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking ||
                PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            // only do this if symlink exists
            /*
            foreach (string path2 in DirOrig.GetDirectories(path))
            {
                MonoIOError monoIOError;
                if (MonoIO.ExistsSymlink(path2, out monoIOError))
                {
                    MonoIO.DeleteFile(path2, out monoIOError);
                }
                else
                {
                    DirOrig.RecursiveDelete(path2);
                }
            }
            string[] array = DirOrig.GetFiles(path);
            for (int i = 0; i < array.Length; i++)
            {
                FileOrig.Delete(array[i]);
            }
            DirOrig.Delete(path);
            */
            FileSystem.OsHelpers.DeleteEmptyDir(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.GetLastAccessTime))]
    private class GetLastAccessTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref DateTimeOrig __result, string path)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking ||
                PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            __result = FileSystem.OsHelpers.DirAccessTime(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.GetLastWriteTime))]
    private class GetLastWriteTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref DateTimeOrig __result, string path)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking ||
                PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            __result = FileSystem.OsHelpers.DirWriteTime(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.GetCreationTime))]
    private class GetCreationTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref DateTimeOrig __result, string path)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking ||
                PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            __result = FileSystem.OsHelpers.DirCreationTime(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), "IsRootDirectory")]
    private class IsRootDirectory
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref bool __result, string path)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking ||
                PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            __result =
                FileSystem.ExternalHelpers.DirectorySeparatorChar == '/' && path == "/" ||
                FileSystem.ExternalHelpers.DirectorySeparatorChar == '\\' && path.Length == 3 && path.EndsWith(":\\");
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.Move))]
    private class Move
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(string sourceDirName, string destDirName)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking ||
                PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            if (sourceDirName == null)
            {
                throw new ArgumentNullException(nameof(sourceDirName));
            }

            if (destDirName == null)
            {
                throw new ArgumentNullException(nameof(destDirName));
            }

            if (sourceDirName.Trim().Length == 0 ||
                sourceDirName.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
            {
                throw new ArgumentException("Invalid source directory name: " + sourceDirName, nameof(sourceDirName));
            }

            if (destDirName.Trim().Length == 0 ||
                destDirName.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
            {
                throw new ArgumentException("Invalid target directory name: " + destDirName, nameof(destDirName));
            }

            if (sourceDirName == destDirName)
            {
                throw new IOException("Source and destination path must be different.");
            }

            if (DirOrig.Exists(destDirName))
            {
                throw new IOException(destDirName + " already exists.");
            }

            if (!DirOrig.Exists(sourceDirName) && !FileOrig.Exists(sourceDirName))
            {
                throw new DirectoryNotFoundException(sourceDirName + " does not exist");
            }

            FileSystem.OsHelpers.MoveDirectory(sourceDirName, destDirName);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.SetAccessControl))]
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

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.SetCreationTime))]
    private class SetCreationTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(string path, DateTimeOrig creationTime)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking ||
                PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            FileSystem.OsHelpers.SetDirCreationTime(path, creationTime);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.SetLastAccessTime))]
    private class SetLastAccessTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(string path, DateTimeOrig lastAccessTime)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking ||
                PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            FileSystem.OsHelpers.SetDirAccessTime(path, lastAccessTime);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.SetLastWriteTime))]
    private class SetLastWriteTime
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(string path, DateTimeOrig lastWriteTime)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking ||
                PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            FileSystem.OsHelpers.SetDirWriteTime(path, lastWriteTime);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), "GetDemandDir")]
    private class GetDemandDir
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref string __result, string fullPath, bool thisDirOnly)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking ||
                PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            var result = thisDirOnly
                ? fullPath.EndsWith(FileSystem.ExternalHelpers.DirectorySeparatorStr) ||
                  fullPath.EndsWith(FileSystem.ExternalHelpers.AltDirectorySeparatorChar.ToString())
                    ? fullPath + "."
                    : fullPath + FileSystem.ExternalHelpers.DirectorySeparatorStr + "."
                : !fullPath.EndsWith(FileSystem.ExternalHelpers.DirectorySeparatorStr) &&
                  !fullPath.EndsWith(FileSystem.ExternalHelpers.AltDirectorySeparatorChar.ToString())
                    ? fullPath + FileSystem.ExternalHelpers.DirectorySeparatorStr
                    : fullPath;
            __result = result;
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.GetAccessControl), typeof(string), typeof(AccessControlSections))]
    private class GetAccessControl
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref DirectorySecurity __result)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking ||
                PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            __result = new();
            __result.AddAccessRule(new("Everyone", FileSystemRights.FullControl,
                AccessControlType.Allow));
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), "InsecureGetCurrentDirectory")]
    private class InsecureGetCurrentDirectory
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref string __result)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking ||
                PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            __result = FileSystem.OsHelpers.WorkingDir().FullName;
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.SetCurrentDirectory))]
    private class SetCurrentDirectory
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(string path)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking ||
                PatcherHelper.InvokedFromCriticalNamespace())
                return true;
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.Trim().Length == 0)
            {
                throw new ArgumentException("path string must not be an empty string or whitespace string");
            }

            if (!DirOrig.Exists(path))
            {
                throw new DirectoryNotFoundException("Directory \"" + path + "\" not found.");
            }

            FileSystem.OsHelpers.SetWorkingDir(path);
            return false;
        }
    }
}