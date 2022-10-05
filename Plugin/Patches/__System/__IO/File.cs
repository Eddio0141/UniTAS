using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using UniTASPlugin.FakeGameState.GameFileSystem;
using FileOrig = System.IO.File;

namespace UniTASPlugin.Patches.__System.__IO;

[HarmonyPatch]
public static class File
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
#pragma warning disable CS0618 // Type or member is obsolete
            __result = !string.IsNullOrEmpty(path) && path.IndexOfAny(Path.InvalidPathChars) < 0 && FileSystem.FileExists(path);
#pragma warning restore CS0618 // Type or member is obsolete
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
}


class Dummy3
{
    /*

	public static DateTime GetCreationTime(string path)
	{
		Path.Validate(path);
		SecurityManager.EnsureElevatedPermissions();
		MonoIOStat monoIOStat;
		MonoIOError monoIOError;
		if (MonoIO.GetFileStat(path, out monoIOStat, out monoIOError))
		{
			return DateTime.FromFileTime(monoIOStat.CreationTime);
		}
		if (monoIOError == MonoIOError.ERROR_PATH_NOT_FOUND || monoIOError == MonoIOError.ERROR_FILE_NOT_FOUND)
		{
			return File.DefaultLocalFileTime;
		}
		throw new IOException(path);
	}

	public static DateTime GetCreationTimeUtc(string path)
	{
		return File.GetCreationTime(path).ToUniversalTime();
	}

	public static DateTime GetLastAccessTime(string path)
	{
		Path.Validate(path);
		SecurityManager.EnsureElevatedPermissions();
		MonoIOStat monoIOStat;
		MonoIOError monoIOError;
		if (MonoIO.GetFileStat(path, out monoIOStat, out monoIOError))
		{
			return DateTime.FromFileTime(monoIOStat.LastAccessTime);
		}
		if (monoIOError == MonoIOError.ERROR_PATH_NOT_FOUND || monoIOError == MonoIOError.ERROR_FILE_NOT_FOUND)
		{
			return File.DefaultLocalFileTime;
		}
		throw new IOException(path);
	}

	public static DateTime GetLastAccessTimeUtc(string path)
	{
		return File.GetLastAccessTime(path).ToUniversalTime();
	}

	public static DateTime GetLastWriteTime(string path)
	{
		Path.Validate(path);
		SecurityManager.EnsureElevatedPermissions();
		MonoIOStat monoIOStat;
		MonoIOError monoIOError;
		if (MonoIO.GetFileStat(path, out monoIOStat, out monoIOError))
		{
			return DateTime.FromFileTime(monoIOStat.LastWriteTime);
		}
		if (monoIOError == MonoIOError.ERROR_PATH_NOT_FOUND || monoIOError == MonoIOError.ERROR_FILE_NOT_FOUND)
		{
			return File.DefaultLocalFileTime;
		}
		throw new IOException(path);
	}

	public static DateTime GetLastWriteTimeUtc(string path)
	{
		return File.GetLastWriteTime(path).ToUniversalTime();
	}

	public static void Move(string sourceFileName, string destFileName)
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
		if (sourceFileName.Trim().Length == 0 || sourceFileName.IndexOfAny(Path.InvalidPathChars) != -1)
		{
			throw new ArgumentException("The file name is not valid.");
		}
		if (destFileName.Length == 0)
		{
			throw new ArgumentException("An empty file name is not valid.", "destFileName");
		}
		if (destFileName.Trim().Length == 0 || destFileName.IndexOfAny(Path.InvalidPathChars) != -1)
		{
			throw new ArgumentException("The file name is not valid.");
		}
		SecurityManager.EnsureElevatedPermissions();
		MonoIOError monoIOError;
		if (!MonoIO.Exists(sourceFileName, out monoIOError))
		{
			throw new FileNotFoundException(Locale.GetText("{0} does not exist", new object[]
			{
					sourceFileName
			}), sourceFileName);
		}
		string directoryName = Path.GetDirectoryName(destFileName);
		if (directoryName != string.Empty && !Directory.Exists(directoryName))
		{
			throw new DirectoryNotFoundException(Locale.GetText("Could not find a part of the path."));
		}
		if (MonoIO.MoveFile(sourceFileName, destFileName, out monoIOError))
		{
			return;
		}
		if (monoIOError == MonoIOError.ERROR_ALREADY_EXISTS)
		{
			throw MonoIO.GetException(monoIOError);
		}
		if (monoIOError == MonoIOError.ERROR_SHARING_VIOLATION)
		{
			throw MonoIO.GetException(sourceFileName, monoIOError);
		}
		throw MonoIO.GetException(monoIOError);
	}

	public static void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName)
	{
		File.Replace(sourceFileName, destinationFileName, destinationBackupFileName, false);
	}

	public static void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
	{
		if (sourceFileName == null)
		{
			throw new ArgumentNullException("sourceFileName");
		}
		if (destinationFileName == null)
		{
			throw new ArgumentNullException("destinationFileName");
		}
		if (sourceFileName.Trim().Length == 0 || sourceFileName.IndexOfAny(Path.InvalidPathChars) != -1)
		{
			throw new ArgumentException("sourceFileName");
		}
		if (destinationFileName.Trim().Length == 0 || destinationFileName.IndexOfAny(Path.InvalidPathChars) != -1)
		{
			throw new ArgumentException("destinationFileName");
		}
		string fullPath = Path.GetFullPath(sourceFileName);
		string fullPath2 = Path.GetFullPath(destinationFileName);
		MonoIOError error;
		if (MonoIO.ExistsDirectory(fullPath, out error))
		{
			throw new IOException(Locale.GetText("{0} is a directory", new object[]
			{
					sourceFileName
			}));
		}
		if (MonoIO.ExistsDirectory(fullPath2, out error))
		{
			throw new IOException(Locale.GetText("{0} is a directory", new object[]
			{
					destinationFileName
			}));
		}
		if (!File.Exists(fullPath))
		{
			throw new FileNotFoundException(Locale.GetText("{0} does not exist", new object[]
			{
					sourceFileName
			}), sourceFileName);
		}
		if (!File.Exists(fullPath2))
		{
			throw new FileNotFoundException(Locale.GetText("{0} does not exist", new object[]
			{
					destinationFileName
			}), destinationFileName);
		}
		if (fullPath == fullPath2)
		{
			throw new IOException(Locale.GetText("Source and destination arguments are the same file."));
		}
		string text = null;
		if (destinationBackupFileName != null)
		{
			if (destinationBackupFileName.Trim().Length == 0 || destinationBackupFileName.IndexOfAny(Path.InvalidPathChars) != -1)
			{
				throw new ArgumentException("destinationBackupFileName");
			}
			text = Path.GetFullPath(destinationBackupFileName);
			if (MonoIO.ExistsDirectory(text, out error))
			{
				throw new IOException(Locale.GetText("{0} is a directory", new object[]
				{
						destinationBackupFileName
				}));
			}
			if (fullPath == text)
			{
				throw new IOException(Locale.GetText("Source and backup arguments are the same file."));
			}
			if (fullPath2 == text)
			{
				throw new IOException(Locale.GetText("Destination and backup arguments are the same file."));
			}
		}
		if ((File.GetAttributes(fullPath2) & FileAttributes.ReadOnly) != (FileAttributes)0)
		{
			throw MonoIO.GetException(MonoIOError.ERROR_ACCESS_DENIED);
		}
		if (!MonoIO.ReplaceFile(fullPath, fullPath2, text, ignoreMetadataErrors, out error))
		{
			throw MonoIO.GetException(error);
		}
	}

	public static void SetCreationTime(string path, DateTime creationTime)
	{
		Path.Validate(path);
		MonoIOError error;
		if (!MonoIO.Exists(path, out error))
		{
			throw MonoIO.GetException(path, error);
		}
		if (!MonoIO.SetCreationTime(path, creationTime, out error))
		{
			throw MonoIO.GetException(path, error);
		}
	}

	public static void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
	{
		File.SetCreationTime(path, creationTimeUtc.ToLocalTime());
	}

	public static void SetLastAccessTime(string path, DateTime lastAccessTime)
	{
		Path.Validate(path);
		MonoIOError error;
		if (!MonoIO.Exists(path, out error))
		{
			throw MonoIO.GetException(path, error);
		}
		if (!MonoIO.SetLastAccessTime(path, lastAccessTime, out error))
		{
			throw MonoIO.GetException(path, error);
		}
	}

	public static void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
	{
		File.SetLastAccessTime(path, lastAccessTimeUtc.ToLocalTime());
	}

	public static void SetLastWriteTime(string path, DateTime lastWriteTime)
	{
		Path.Validate(path);
		MonoIOError error;
		if (!MonoIO.Exists(path, out error))
		{
			throw MonoIO.GetException(path, error);
		}
		if (!MonoIO.SetLastWriteTime(path, lastWriteTime, out error))
		{
			throw MonoIO.GetException(path, error);
		}
	}

	public static void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
	{
		File.SetLastWriteTime(path, lastWriteTimeUtc.ToLocalTime());
	}
    */
}
