using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using UniTASPlugin.FakeGameState.GameFileSystem;
using DirOrig = System.IO.Directory;
using FileOrig = System.IO.File;

namespace UniTASPlugin.Patches.__System.__IO;

[HarmonyPatch]
static class Directory
{
    static class Helper
    {
        static readonly Traverse pathValidateTraverse = Traverse.Create(typeof(Path)).Method("Validate");
        static readonly Traverse environmentIsRunningOnWindowsTraverse = Traverse.Create(typeof(Environment)).Property("IsRunningOnWindows");

        public static void PathValidate(string path)
        {
            pathValidateTraverse.GetValue(path);
        }

        public static bool EnvironmentIsRunningOnWindows()
        {
            return environmentIsRunningOnWindowsTraverse.GetValue<bool>();
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.Exists))]
    class DirExists
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string path, ref bool __result)
        {
            __result = FileSystem.DirectoryExists(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), "InternalGetFileDirectoryNames")]
    class InternalGetFileDirectoryNames
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref string[] __result, string path, string searchPattern, bool includeFiles, bool includeDirs, SearchOption searchOption)
        {
            __result = FileSystem.OsHelpers.GetPaths(path, searchPattern, includeFiles, includeDirs, searchOption);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), "EnumerateFileSystemNames")]
    class EnumerateFileSystemNames
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref IEnumerable<string> __result, string path, string searchPattern, SearchOption searchOption, bool includeFiles, bool includeDirs)
        {
            __result = FileSystem.OsHelpers.GetPaths(path, searchPattern, includeFiles, includeDirs, searchOption);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.GetDirectoryRoot))]
    class GetDirectoryRoot
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref string __result, string path)
        {
            Helper.PathValidate(path);
            __result = new string(FileSystem.ExternalHelpers.DirectorySeparatorChar, 1);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.CreateDirectory), new Type[] { typeof(string) })]
    class CreateDirectory
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref DirectoryInfo __result, string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
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
            __result = Traverse.Create(typeof(DirOrig)).Method("CreateDirectoriesInternal").GetValue<DirectoryInfo>(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), "CreateDirectoriesInternal")]
    class CreateDirectoriesInternal
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref DirectoryInfo __result, string path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path, true);
            if (directoryInfo.Parent != null && !directoryInfo.Parent.Exists)
            {
                directoryInfo.Parent.Create();
            }
            MonoIOError monoIOError;
            if (!MonoIO.CreateDirectory(directoryInfo.FullName, out monoIOError) && monoIOError != MonoIOError.ERROR_ALREADY_EXISTS && monoIOError != MonoIOError.ERROR_FILE_EXISTS)
            {
                throw MonoIO.GetException(path, monoIOError);
            }
            __result = directoryInfo;
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.Delete), new Type[] { typeof(string) })]
    class Delete
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string path)
        {
            Helper.PathValidate(path);
            if (Helper.EnvironmentIsRunningOnWindows() && path == ":")
            {
                throw new NotSupportedException("Only ':' In path");
            }
            MonoIOError monoIOError;
            bool flag;
            if (MonoIO.ExistsSymlink(path, out monoIOError))
            {
                flag = MonoIO.DeleteFile(path, out monoIOError);
            }
            else
            {
                flag = MonoIO.RemoveDirectory(path, out monoIOError);
            }
            if (flag)
            {
                return false;
            }
            if (monoIOError != MonoIOError.ERROR_FILE_NOT_FOUND)
            {
                throw MonoIO.GetException(path, monoIOError);
            }
            if (FileOrig.Exists(path))
            {
                throw new IOException("Directory does not exist, but a file of the same name exists.");
            }
            throw new DirectoryNotFoundException("Directory does not exist.");
        }
    }

    [HarmonyPatch(typeof(DirOrig), "RecursiveDelete")]
    class RecursiveDelete
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string path)
        {
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
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.GetLastAccessTime))]
    class GetLastAccessTime
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref DateTime __result, string path)
        {
            __result = FileSystem.OsHelpers.DirAccessTime(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.GetLastWriteTime))]
    class GetLastWriteTime
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref DateTime __result, string path)
        {
            __result = FileSystem.OsHelpers.DirWriteTime(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.GetCreationTime))]
    class GetCreationTime
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref DateTime __result, string path)
        {
            __result = FileSystem.OsHelpers.DirCreationTime(path);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), "IsRootDirectory")]
    class IsRootDirectory
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref bool __result, string path)
        {
            __result =
                FileSystem.ExternalHelpers.DirectorySeparatorChar == '/' && path == "/" ||
                FileSystem.ExternalHelpers.DirectorySeparatorChar == '\\' && path.Length == 3 && path.EndsWith(":\\");
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.Move))]
    class Move
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string sourceDirName, string destDirName)
        {
            if (sourceDirName == null)
            {
                throw new ArgumentNullException("sourceDirName");
            }
            if (destDirName == null)
            {
                throw new ArgumentNullException("destDirName");
            }
            if (sourceDirName.Trim().Length == 0 || sourceDirName.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
            {
                throw new ArgumentException("Invalid source directory name: " + sourceDirName, "sourceDirName");
            }
            if (destDirName.Trim().Length == 0 || destDirName.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
            {
                throw new ArgumentException("Invalid target directory name: " + destDirName, "destDirName");
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

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.SetCreationTime))]
    class SetCreationTime
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string path, DateTime creationTime)
        {
            FileSystem.OsHelpers.SetDirCreationTime(path, creationTime);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.SetLastAccessTime))]
    class SetLastAccessTime
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string path, DateTime lastAccessTime)
        {
            FileSystem.OsHelpers.SetDirAccessTime(path, lastAccessTime);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.SetLastWriteTime))]
    class SetLastWriteTime
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string path, DateTime lastWriteTime)
        {
            FileSystem.OsHelpers.SetDirWriteTime(path, lastWriteTime);
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), "GetDemandDir")]
    class GetDemandDir
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref string __result, string fullPath, bool thisDirOnly)
        {
            string result;
            if (thisDirOnly)
            {
                if (fullPath.EndsWith(FileSystem.ExternalHelpers.DirectorySeparatorStr) || fullPath.EndsWith(FileSystem.ExternalHelpers.AltDirectorySeparatorChar.ToString()))
                {
                    result = fullPath + ".";
                }
                else
                {
                    result = fullPath + FileSystem.ExternalHelpers.DirectorySeparatorStr + ".";
                }
            }
            else if (!fullPath.EndsWith(FileSystem.ExternalHelpers.DirectorySeparatorStr) && !fullPath.EndsWith(FileSystem.ExternalHelpers.AltDirectorySeparatorChar.ToString()))
            {
                result = fullPath + FileSystem.ExternalHelpers.DirectorySeparatorStr;
            }
            else
            {
                result = fullPath;
            }
            __result = result;
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.GetAccessControl), new Type[] { typeof(string), typeof(AccessControlSections) })]
    class GetAccessControl
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref DirectorySecurity __result)
        {
            __result = new DirectorySecurity();
            __result.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Allow));
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), "InsecureGetCurrentDirectory")]
    class InsecureGetCurrentDirectory
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref string __result)
        {
            __result = FileSystem.OsHelpers.WorkingDir().FullName;
            return false;
        }
    }

    [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.SetCurrentDirectory))]
    class SetCurrentDirectory
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
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