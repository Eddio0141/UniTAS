using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UniTASPlugin.FakeGameState.GameFileSystem;
using FileOrig = System.IO.File;

namespace UniTASPlugin.Patches.__System.__IO;

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

class Dummy3
{
    /*
	/// <summary>Appends the specified string to the file, creating the file if it does not already exist.</summary>
	/// <param name="path">The file to append the specified string to.</param>
	/// <param name="contents">The string to append to the file.</param>
	/// <param name="encoding">The character encoding to use.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, the directory doesn't exist or it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.  
	/// -or-  
	/// This operation is not supported on the current platform.  
	/// -or-  
	/// <paramref name="path" /> specified a directory.  
	/// -or-  
	/// The caller does not have the required permission.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	// Token: 0x06002A5E RID: 10846 RVA: 0x000949B8 File Offset: 0x00092BB8
	public static void AppendAllText(string path, string contents, Encoding encoding)
	{
		using (TextWriter textWriter = new StreamWriter(path, true, encoding))
		{
			textWriter.Write(contents);
		}
	}

	/// <summary>Creates a <see cref="T:System.IO.StreamWriter" /> that appends UTF-8 encoded text to an existing file, or to a new file if the specified file does not exist.</summary>
	/// <param name="path">The path to the file to append to.</param>
	/// <returns>A stream writer that appends UTF-8 encoded text to the specified file or to a new file.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, the directory doesn't exist or it is on an unmapped drive).</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	// Token: 0x06002A5F RID: 10847 RVA: 0x000949F4 File Offset: 0x00092BF4
	public static StreamWriter AppendText(string path)
	{
		return new StreamWriter(path, true);
	}

	/// <summary>Copies an existing file to a new file. Overwriting a file of the same name is not allowed.</summary>
	/// <param name="sourceFileName">The file to copy.</param>
	/// <param name="destFileName">The name of the destination file. This cannot be a directory or an existing file.</param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.  
	/// -or-  
	/// <paramref name="sourceFileName" /> or <paramref name="destFileName" /> specifies a directory.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The path specified in <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="sourceFileName" /> was not found.</exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="destFileName" /> exists.  
	/// -or-  
	/// An I/O error has occurred.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is in an invalid format.</exception>
	// Token: 0x06002A60 RID: 10848 RVA: 0x000949FD File Offset: 0x00092BFD
	public static void Copy(string sourceFileName, string destFileName)
	{
		File.Copy(sourceFileName, destFileName, false);
	}

	/// <summary>Copies an existing file to a new file. Overwriting a file of the same name is allowed.</summary>
	/// <param name="sourceFileName">The file to copy.</param>
	/// <param name="destFileName">The name of the destination file. This cannot be a directory.</param>
	/// <param name="overwrite">
	///   <see langword="true" /> if the destination file can be overwritten; otherwise, <see langword="false" />.</param>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.  
	///  -or-  
	///  <paramref name="destFileName" /> is read-only.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.  
	/// -or-  
	/// <paramref name="sourceFileName" /> or <paramref name="destFileName" /> specifies a directory.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The path specified in <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="sourceFileName" /> was not found.</exception>
	/// <exception cref="T:System.IO.IOException">
	///   <paramref name="destFileName" /> exists and <paramref name="overwrite" /> is <see langword="false" />.  
	/// -or-  
	/// An I/O error has occurred.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is in an invalid format.</exception>
	// Token: 0x06002A61 RID: 10849 RVA: 0x00094A08 File Offset: 0x00092C08
	public static void Copy(string sourceFileName, string destFileName, bool overwrite)
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
		MonoIOError error;
		if (!MonoIO.Exists(sourceFileName, out error))
		{
			throw new FileNotFoundException(Locale.GetText("{0} does not exist", new object[]
			{
					sourceFileName
			}), sourceFileName);
		}
		if ((File.GetAttributes(sourceFileName) & FileAttributes.Directory) == FileAttributes.Directory)
		{
			throw new ArgumentException(Locale.GetText("{0} is a directory", new object[]
			{
					sourceFileName
			}));
		}
		if (MonoIO.Exists(destFileName, out error))
		{
			if ((File.GetAttributes(destFileName) & FileAttributes.Directory) == FileAttributes.Directory)
			{
				throw new ArgumentException(Locale.GetText("{0} is a directory", new object[]
				{
						destFileName
				}));
			}
			if (!overwrite)
			{
				throw new IOException(Locale.GetText("{0} already exists", new object[]
				{
						destFileName
				}));
			}
		}
		string directoryName = Path.GetDirectoryName(destFileName);
		if (directoryName != string.Empty && !Directory.Exists(directoryName))
		{
			throw new DirectoryNotFoundException(Locale.GetText("Destination directory not found: {0}", new object[]
			{
					directoryName
			}));
		}
		if (!MonoIO.CopyFile(sourceFileName, destFileName, overwrite, out error))
		{
			throw MonoIO.GetException(Locale.GetText("{0}\" or \"{1}", new object[]
			{
					sourceFileName,
					destFileName
			}), error);
		}
	}

	// Token: 0x06002A62 RID: 10850 RVA: 0x00094BB0 File Offset: 0x00092DB0
	internal static string InternalCopy(string sourceFileName, string destFileName, bool overwrite, bool checkHost)
	{
		string fullPathInternal = Path.GetFullPathInternal(sourceFileName);
		string fullPathInternal2 = Path.GetFullPathInternal(destFileName);
		MonoIOError error;
		if (!MonoIO.CopyFile(fullPathInternal, fullPathInternal2, overwrite, out error))
		{
			throw MonoIO.GetException(Locale.GetText("{0}\" or \"{1}", new object[]
			{
					sourceFileName,
					destFileName
			}), error);
		}
		return fullPathInternal2;
	}

	/// <summary>Creates or overwrites a file in the specified path.</summary>
	/// <param name="path">The path and name of the file to create.</param>
	/// <returns>A <see cref="T:System.IO.FileStream" /> that provides read/write access to the file specified in <paramref name="path" />.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.  
	///  -or-  
	///  <paramref name="path" /> specified a file that is read-only.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while creating the file.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	// Token: 0x06002A63 RID: 10851 RVA: 0x00094BF5 File Offset: 0x00092DF5
	public static FileStream Create(string path)
	{
		return File.Create(path, 8192);
	}

	/// <summary>Creates or overwrites the specified file.</summary>
	/// <param name="path">The name of the file.</param>
	/// <param name="bufferSize">The number of bytes buffered for reads and writes to the file.</param>
	/// <returns>A <see cref="T:System.IO.FileStream" /> with the specified buffer size that provides read/write access to the file specified in <paramref name="path" />.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.  
	///  -or-  
	///  <paramref name="path" /> specified a file that is read-only.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while creating the file.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	// Token: 0x06002A64 RID: 10852 RVA: 0x00094C02 File Offset: 0x00092E02
	public static FileStream Create(string path, int bufferSize)
	{
		return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize);
	}

	/// <summary>Creates or overwrites the specified file, specifying a buffer size and a <see cref="T:System.IO.FileOptions" /> value that describes how to create or overwrite the file.</summary>
	/// <param name="path">The name of the file.</param>
	/// <param name="bufferSize">The number of bytes buffered for reads and writes to the file.</param>
	/// <param name="options">One of the <see cref="T:System.IO.FileOptions" /> values that describes how to create or overwrite the file.</param>
	/// <returns>A new file with the specified buffer size.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.  
	///  -or-  
	///  <paramref name="path" /> specified a file that is read-only.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while creating the file.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	// Token: 0x06002A65 RID: 10853 RVA: 0x00094C0E File Offset: 0x00092E0E
	[MonoLimitation("FileOptions are ignored")]
	public static FileStream Create(string path, int bufferSize, FileOptions options)
	{
		return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize, options);
	}

	/// <summary>Creates or overwrites the specified file with the specified buffer size, file options, and file security.</summary>
	/// <param name="path">The name of the file.</param>
	/// <param name="bufferSize">The number of bytes buffered for reads and writes to the file.</param>
	/// <param name="options">One of the <see cref="T:System.IO.FileOptions" /> values that describes how to create or overwrite the file.</param>
	/// <param name="fileSecurity">One of the <see cref="T:System.Security.AccessControl.FileSecurity" /> values that determines the access control and audit security for the file.</param>
	/// <returns>A new file with the specified buffer size, file options, and file security.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.  
	///  -or-  
	///  <paramref name="path" /> specified a file that is read-only.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while creating the file.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	// Token: 0x06002A66 RID: 10854 RVA: 0x00094C0E File Offset: 0x00092E0E
	[MonoLimitation("FileOptions and FileSecurity are ignored")]
	public static FileStream Create(string path, int bufferSize, FileOptions options, FileSecurity fileSecurity)
	{
		return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize, options);
	}

	/// <summary>Creates or opens a file for writing UTF-8 encoded text. If the file already exists, its contents are overwritten.</summary>
	/// <param name="path">The file to be opened for writing.</param>
	/// <returns>A <see cref="T:System.IO.StreamWriter" /> that writes to the specified file using UTF-8 encoding.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	// Token: 0x06002A67 RID: 10855 RVA: 0x00094C1B File Offset: 0x00092E1B
	public static StreamWriter CreateText(string path)
	{
		return new StreamWriter(path, false);
	}

	/// <summary>Deletes the specified file.</summary>
	/// <param name="path">The name of the file to be deleted. Wildcard characters are not supported.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">The specified file is in use.  
	///  -or-  
	///  There is an open handle on the file, and the operating system is Windows XP or earlier. This open handle can result from enumerating directories and files. For more information, see How to: Enumerate Directories and Files.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.  
	///  -or-  
	///  The file is an executable file that is in use.  
	///  -or-  
	///  <paramref name="path" /> is a directory.  
	///  -or-  
	///  <paramref name="path" /> specified a read-only file.</exception>
	// Token: 0x06002A68 RID: 10856 RVA: 0x00094C24 File Offset: 0x00092E24
	public static void Delete(string path)
	{
		Path.Validate(path);
		if (Directory.Exists(path))
		{
			throw new UnauthorizedAccessException(Locale.GetText("{0} is a directory", new object[]
			{
					path
			}));
		}
		string directoryName = Path.GetDirectoryName(path);
		if (directoryName != string.Empty && !Directory.Exists(directoryName))
		{
			throw new DirectoryNotFoundException(Locale.GetText("Could not find a part of the path \"{0}\".", new object[]
			{
					path
			}));
		}
		SecurityManager.EnsureElevatedPermissions();
		MonoIOError monoIOError;
		if (!MonoIO.DeleteFile(path, out monoIOError) && monoIOError != MonoIOError.ERROR_FILE_NOT_FOUND)
		{
			throw MonoIO.GetException(path, monoIOError);
		}
	}

	/// <summary>Gets a <see cref="T:System.Security.AccessControl.FileSecurity" /> object that encapsulates the access control list (ACL) entries for a specified file.</summary>
	/// <param name="path">The path to a file containing a <see cref="T:System.Security.AccessControl.FileSecurity" /> object that describes the file's access control list (ACL) information.</param>
	/// <returns>A <see cref="T:System.Security.AccessControl.FileSecurity" /> object that encapsulates the access control rules for the file described by the <paramref name="path" /> parameter.</returns>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.SEHException">The <paramref name="path" /> parameter is <see langword="null" />.</exception>
	/// <exception cref="T:System.SystemException">The file could not be found.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="path" /> parameter specified a file that is read-only.  
	///  -or-  
	///  This operation is not supported on the current platform.  
	///  -or-  
	///  The <paramref name="path" /> parameter specified a directory.  
	///  -or-  
	///  The caller does not have the required permission.</exception>
	// Token: 0x06002A6A RID: 10858 RVA: 0x00094CE2 File Offset: 0x00092EE2
	public static FileSecurity GetAccessControl(string path)
	{
		return File.GetAccessControl(path, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
	}

	/// <summary>Gets a <see cref="T:System.Security.AccessControl.FileSecurity" /> object that encapsulates the specified type of access control list (ACL) entries for a particular file.</summary>
	/// <param name="path">The path to a file containing a <see cref="T:System.Security.AccessControl.FileSecurity" /> object that describes the file's access control list (ACL) information.</param>
	/// <param name="includeSections">One of the <see cref="T:System.Security.AccessControl.AccessControlSections" /> values that specifies the type of access control list (ACL) information to receive.</param>
	/// <returns>A <see cref="T:System.Security.AccessControl.FileSecurity" /> object that encapsulates the access control rules for the file described by the <paramref name="path" /> parameter.</returns>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.SEHException">The <paramref name="path" /> parameter is <see langword="null" />.</exception>
	/// <exception cref="T:System.SystemException">The file could not be found.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="path" /> parameter specified a file that is read-only.  
	///  -or-  
	///  This operation is not supported on the current platform.  
	///  -or-  
	///  The <paramref name="path" /> parameter specified a directory.  
	///  -or-  
	///  The caller does not have the required permission.</exception>
	// Token: 0x06002A6B RID: 10859 RVA: 0x00094CEC File Offset: 0x00092EEC
	public static FileSecurity GetAccessControl(string path, AccessControlSections includeSections)
	{
		return new FileSecurity(path, includeSections);
	}

	/// <summary>Gets the <see cref="T:System.IO.FileAttributes" /> of the file on the path.</summary>
	/// <param name="path">The path to the file.</param>
	/// <returns>The <see cref="T:System.IO.FileAttributes" /> of the file on the path.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is empty, contains only white spaces, or contains invalid characters.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">
	///   <paramref name="path" /> represents a file and is invalid, such as being on an unmapped drive, or the file cannot be found.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> represents a directory and is invalid, such as being on an unmapped drive, or the directory cannot be found.</exception>
	/// <exception cref="T:System.IO.IOException">This file is being used by another process.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	// Token: 0x06002A6C RID: 10860 RVA: 0x00094CF8 File Offset: 0x00092EF8
	public static FileAttributes GetAttributes(string path)
	{
		Path.Validate(path);
		SecurityManager.EnsureElevatedPermissions();
		MonoIOError monoIOError;
		FileAttributes fileAttributes = MonoIO.GetFileAttributes(path, out monoIOError);
		if (monoIOError != MonoIOError.ERROR_SUCCESS)
		{
			throw MonoIO.GetException(path, monoIOError);
		}
		return fileAttributes;
	}

	/// <summary>Returns the creation date and time of the specified file or directory.</summary>
	/// <param name="path">The file or directory for which to obtain creation date and time information.</param>
	/// <returns>A <see cref="T:System.DateTime" /> structure set to the creation date and time for the specified file or directory. This value is expressed in local time.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	// Token: 0x06002A6D RID: 10861 RVA: 0x00094D24 File Offset: 0x00092F24
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

	/// <summary>Returns the creation date and time, in coordinated universal time (UTC), of the specified file or directory.</summary>
	/// <param name="path">The file or directory for which to obtain creation date and time information.</param>
	/// <returns>A <see cref="T:System.DateTime" /> structure set to the creation date and time for the specified file or directory. This value is expressed in UTC time.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	// Token: 0x06002A6E RID: 10862 RVA: 0x00094D68 File Offset: 0x00092F68
	public static DateTime GetCreationTimeUtc(string path)
	{
		return File.GetCreationTime(path).ToUniversalTime();
	}

	/// <summary>Returns the date and time the specified file or directory was last accessed.</summary>
	/// <param name="path">The file or directory for which to obtain access date and time information.</param>
	/// <returns>A <see cref="T:System.DateTime" /> structure set to the date and time that the specified file or directory was last accessed. This value is expressed in local time.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	// Token: 0x06002A6F RID: 10863 RVA: 0x00094D84 File Offset: 0x00092F84
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

	/// <summary>Returns the date and time, in coordinated universal time (UTC), that the specified file or directory was last accessed.</summary>
	/// <param name="path">The file or directory for which to obtain access date and time information.</param>
	/// <returns>A <see cref="T:System.DateTime" /> structure set to the date and time that the specified file or directory was last accessed. This value is expressed in UTC time.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	// Token: 0x06002A70 RID: 10864 RVA: 0x00094DC8 File Offset: 0x00092FC8
	public static DateTime GetLastAccessTimeUtc(string path)
	{
		return File.GetLastAccessTime(path).ToUniversalTime();
	}

	/// <summary>Returns the date and time the specified file or directory was last written to.</summary>
	/// <param name="path">The file or directory for which to obtain write date and time information.</param>
	/// <returns>A <see cref="T:System.DateTime" /> structure set to the date and time that the specified file or directory was last written to. This value is expressed in local time.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	// Token: 0x06002A71 RID: 10865 RVA: 0x00094DE4 File Offset: 0x00092FE4
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

	/// <summary>Returns the date and time, in coordinated universal time (UTC), that the specified file or directory was last written to.</summary>
	/// <param name="path">The file or directory for which to obtain write date and time information.</param>
	/// <returns>A <see cref="T:System.DateTime" /> structure set to the date and time that the specified file or directory was last written to. This value is expressed in UTC time.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	// Token: 0x06002A72 RID: 10866 RVA: 0x00094E28 File Offset: 0x00093028
	public static DateTime GetLastWriteTimeUtc(string path)
	{
		return File.GetLastWriteTime(path).ToUniversalTime();
	}

	/// <summary>Moves a specified file to a new location, providing the option to specify a new file name.</summary>
	/// <param name="sourceFileName">The name of the file to move. Can include a relative or absolute path.</param>
	/// <param name="destFileName">The new path and name for the file.</param>
	/// <exception cref="T:System.IO.IOException">The destination file already exists.  
	///  -or-  
	///  <paramref name="sourceFileName" /> was not found.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is a zero-length string, contains only white space, or contains invalid characters as defined in <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The path specified in <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is invalid, (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="sourceFileName" /> or <paramref name="destFileName" /> is in an invalid format.</exception>
	// Token: 0x06002A73 RID: 10867 RVA: 0x00094E44 File Offset: 0x00093044
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

	/// <summary>Opens a <see cref="T:System.IO.FileStream" /> on the specified path with read/write access with no sharing.</summary>
	/// <param name="path">The file to open.</param>
	/// <param name="mode">A <see cref="T:System.IO.FileMode" /> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
	/// <returns>A <see cref="T:System.IO.FileStream" /> opened in the specified mode and path, with read/write access and not shared.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.  
	/// -or-  
	/// This operation is not supported on the current platform.  
	/// -or-  
	/// <paramref name="path" /> specified a directory.  
	/// -or-  
	/// The caller does not have the required permission.  
	/// -or-  
	/// <paramref name="mode" /> is <see cref="F:System.IO.FileMode.Create" /> and the specified file is a hidden file.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="mode" /> specified an invalid value.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	// Token: 0x06002A74 RID: 10868 RVA: 0x00094F6D File Offset: 0x0009316D
	public static FileStream Open(string path, FileMode mode)
	{
		return new FileStream(path, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, FileShare.None);
	}

	/// <summary>Opens a <see cref="T:System.IO.FileStream" /> on the specified path, with the specified mode and access with no sharing.</summary>
	/// <param name="path">The file to open.</param>
	/// <param name="mode">A <see cref="T:System.IO.FileMode" /> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
	/// <param name="access">A <see cref="T:System.IO.FileAccess" /> value that specifies the operations that can be performed on the file.</param>
	/// <returns>An unshared <see cref="T:System.IO.FileStream" /> that provides access to the specified file, with the specified mode and access.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.  
	/// -or-  
	/// <paramref name="access" /> specified <see langword="Read" /> and <paramref name="mode" /> specified <see langword="Create" />, <see langword="CreateNew" />, <see langword="Truncate" />, or <see langword="Append" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only and <paramref name="access" /> is not <see langword="Read" />.  
	/// -or-  
	/// <paramref name="path" /> specified a directory.  
	/// -or-  
	/// The caller does not have the required permission.  
	/// -or-  
	/// <paramref name="mode" /> is <see cref="F:System.IO.FileMode.Create" /> and the specified file is a hidden file.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="mode" /> or <paramref name="access" /> specified an invalid value.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	// Token: 0x06002A75 RID: 10869 RVA: 0x00094F7F File Offset: 0x0009317F
	public static FileStream Open(string path, FileMode mode, FileAccess access)
	{
		return new FileStream(path, mode, access, FileShare.None);
	}

	/// <summary>Opens a <see cref="T:System.IO.FileStream" /> on the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.</summary>
	/// <param name="path">The file to open.</param>
	/// <param name="mode">A <see cref="T:System.IO.FileMode" /> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
	/// <param name="access">A <see cref="T:System.IO.FileAccess" /> value that specifies the operations that can be performed on the file.</param>
	/// <param name="share">A <see cref="T:System.IO.FileShare" /> value specifying the type of access other threads have to the file.</param>
	/// <returns>A <see cref="T:System.IO.FileStream" /> on the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.  
	/// -or-  
	/// <paramref name="access" /> specified <see langword="Read" /> and <paramref name="mode" /> specified <see langword="Create" />, <see langword="CreateNew" />, <see langword="Truncate" />, or <see langword="Append" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only and <paramref name="access" /> is not <see langword="Read" />.  
	/// -or-  
	/// <paramref name="path" /> specified a directory.  
	/// -or-  
	/// The caller does not have the required permission.  
	/// -or-  
	/// <paramref name="mode" /> is <see cref="F:System.IO.FileMode.Create" /> and the specified file is a hidden file.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="mode" />, <paramref name="access" />, or <paramref name="share" /> specified an invalid value.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	// Token: 0x06002A76 RID: 10870 RVA: 0x00094F8A File Offset: 0x0009318A
	public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
	{
		return new FileStream(path, mode, access, share);
	}

	/// <summary>Opens an existing file for reading.</summary>
	/// <param name="path">The file to be opened for reading.</param>
	/// <returns>A read-only <see cref="T:System.IO.FileStream" /> on the specified path.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a directory.  
	/// -or-  
	/// The caller does not have the required permission.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	// Token: 0x06002A77 RID: 10871 RVA: 0x00094F95 File Offset: 0x00093195
	public static FileStream OpenRead(string path)
	{
		return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
	}

	/// <summary>Opens an existing UTF-8 encoded text file for reading.</summary>
	/// <param name="path">The file to be opened for reading.</param>
	/// <returns>A <see cref="T:System.IO.StreamReader" /> on the specified path.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	// Token: 0x06002A78 RID: 10872 RVA: 0x00094FA0 File Offset: 0x000931A0
	public static StreamReader OpenText(string path)
	{
		return new StreamReader(path);
	}

	/// <summary>Opens an existing file or creates a new file for writing.</summary>
	/// <param name="path">The file to be opened for writing.</param>
	/// <returns>An unshared <see cref="T:System.IO.FileStream" /> object on the specified path with <see cref="F:System.IO.FileAccess.Write" /> access.</returns>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.  
	///  -or-  
	///  <paramref name="path" /> specified a read-only file or directory.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	// Token: 0x06002A79 RID: 10873 RVA: 0x00094FA8 File Offset: 0x000931A8
	public static FileStream OpenWrite(string path)
	{
		return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
	}

	/// <summary>Replaces the contents of a specified file with the contents of another file, deleting the original file, and creating a backup of the replaced file.</summary>
	/// <param name="sourceFileName">The name of a file that replaces the file specified by <paramref name="destinationFileName" />.</param>
	/// <param name="destinationFileName">The name of the file being replaced.</param>
	/// <param name="destinationBackupFileName">The name of the backup file.</param>
	/// <exception cref="T:System.ArgumentException">The path described by the <paramref name="destinationFileName" /> parameter was not of a legal form.  
	///  -or-  
	///  The path described by the <paramref name="destinationBackupFileName" /> parameter was not of a legal form.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="destinationFileName" /> parameter is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.DriveNotFoundException">An invalid drive was specified.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file described by the current <see cref="T:System.IO.FileInfo" /> object could not be found.  
	///  -or-  
	///  The file described by the <paramref name="destinationBackupFileName" /> parameter could not be found.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.  
	/// -or-
	///  The <paramref name="sourceFileName" /> and <paramref name="destinationFileName" /> parameters specify the same file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows 98 Second Edition or earlier and the files system is not NTFS.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="sourceFileName" /> or <paramref name="destinationFileName" /> parameter specifies a file that is read-only.  
	///  -or-  
	///  This operation is not supported on the current platform.  
	///  -or-  
	///  Source or destination parameters specify a directory instead of a file.  
	///  -or-  
	///  The caller does not have the required permission.</exception>
	// Token: 0x06002A7A RID: 10874 RVA: 0x00094FB3 File Offset: 0x000931B3
	public static void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName)
	{
		File.Replace(sourceFileName, destinationFileName, destinationBackupFileName, false);
	}

	/// <summary>Replaces the contents of a specified file with the contents of another file, deleting the original file, and creating a backup of the replaced file and optionally ignores merge errors.</summary>
	/// <param name="sourceFileName">The name of a file that replaces the file specified by <paramref name="destinationFileName" />.</param>
	/// <param name="destinationFileName">The name of the file being replaced.</param>
	/// <param name="destinationBackupFileName">The name of the backup file.</param>
	/// <param name="ignoreMetadataErrors">
	///   <see langword="true" /> to ignore merge errors (such as attributes and access control lists (ACLs)) from the replaced file to the replacement file; otherwise, <see langword="false" />.</param>
	/// <exception cref="T:System.ArgumentException">The path described by the <paramref name="destinationFileName" /> parameter was not of a legal form.  
	///  -or-  
	///  The path described by the <paramref name="destinationBackupFileName" /> parameter was not of a legal form.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="destinationFileName" /> parameter is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.DriveNotFoundException">An invalid drive was specified.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file described by the current <see cref="T:System.IO.FileInfo" /> object could not be found.  
	///  -or-  
	///  The file described by the <paramref name="destinationBackupFileName" /> parameter could not be found.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.  
	/// -or-
	///  The <paramref name="sourceFileName" /> and <paramref name="destinationFileName" /> parameters specify the same file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The operating system is Windows 98 Second Edition or earlier and the files system is not NTFS.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="sourceFileName" /> or <paramref name="destinationFileName" /> parameter specifies a file that is read-only.  
	///  -or-  
	///  This operation is not supported on the current platform.  
	///  -or-  
	///  Source or destination parameters specify a directory instead of a file.  
	///  -or-  
	///  The caller does not have the required permission.</exception>
	// Token: 0x06002A7B RID: 10875 RVA: 0x00094FC0 File Offset: 0x000931C0
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

	/// <summary>Applies access control list (ACL) entries described by a <see cref="T:System.Security.AccessControl.FileSecurity" /> object to the specified file.</summary>
	/// <param name="path">A file to add or remove access control list (ACL) entries from.</param>
	/// <param name="fileSecurity">A <see cref="T:System.Security.AccessControl.FileSecurity" /> object that describes an ACL entry to apply to the file described by the <paramref name="path" /> parameter.</param>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.Runtime.InteropServices.SEHException">The <paramref name="path" /> parameter is <see langword="null" />.</exception>
	/// <exception cref="T:System.SystemException">The file could not be found.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="path" /> parameter specified a file that is read-only.  
	///  -or-  
	///  This operation is not supported on the current platform.  
	///  -or-  
	///  The <paramref name="path" /> parameter specified a directory.  
	///  -or-  
	///  The caller does not have the required permission.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="fileSecurity" /> parameter is <see langword="null" />.</exception>
	// Token: 0x06002A7C RID: 10876 RVA: 0x0009519A File Offset: 0x0009339A
	public static void SetAccessControl(string path, FileSecurity fileSecurity)
	{
		if (fileSecurity == null)
		{
			throw new ArgumentNullException("fileSecurity");
		}
		fileSecurity.PersistModifications(path);
	}

	/// <summary>Sets the specified <see cref="T:System.IO.FileAttributes" /> of the file on the specified path.</summary>
	/// <param name="path">The path to the file.</param>
	/// <param name="fileAttributes">A bitwise combination of the enumeration values.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is empty, contains only white spaces, contains invalid characters, or the file attribute is invalid.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file cannot be found.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.  
	/// -or-  
	/// This operation is not supported on the current platform.  
	/// -or-  
	/// <paramref name="path" /> specified a directory.  
	/// -or-  
	/// The caller does not have the required permission.</exception>
	// Token: 0x06002A7D RID: 10877 RVA: 0x000951B4 File Offset: 0x000933B4
	public static void SetAttributes(string path, FileAttributes fileAttributes)
	{
		Path.Validate(path);
		MonoIOError error;
		if (!MonoIO.SetFileAttributes(path, fileAttributes, out error))
		{
			throw MonoIO.GetException(path, error);
		}
	}

	/// <summary>Sets the date and time the file was created.</summary>
	/// <param name="path">The file for which to set the creation date and time information.</param>
	/// <param name="creationTime">A <see cref="T:System.DateTime" /> containing the value to set for the creation date and time of <paramref name="path" />. This value is expressed in local time.</param>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while performing the operation.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="creationTime" /> specifies a value outside the range of dates, times, or both permitted for this operation.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	// Token: 0x06002A7E RID: 10878 RVA: 0x000951DC File Offset: 0x000933DC
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

	/// <summary>Sets the date and time, in coordinated universal time (UTC), that the file was created.</summary>
	/// <param name="path">The file for which to set the creation date and time information.</param>
	/// <param name="creationTimeUtc">A <see cref="T:System.DateTime" /> containing the value to set for the creation date and time of <paramref name="path" />. This value is expressed in UTC time.</param>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while performing the operation.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="creationTime" /> specifies a value outside the range of dates, times, or both permitted for this operation.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	// Token: 0x06002A7F RID: 10879 RVA: 0x00095214 File Offset: 0x00093414
	public static void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
	{
		File.SetCreationTime(path, creationTimeUtc.ToLocalTime());
	}

	/// <summary>Sets the date and time the specified file was last accessed.</summary>
	/// <param name="path">The file for which to set the access date and time information.</param>
	/// <param name="lastAccessTime">A <see cref="T:System.DateTime" /> containing the value to set for the last access date and time of <paramref name="path" />. This value is expressed in local time.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="lastAccessTime" /> specifies a value outside the range of dates or times permitted for this operation.</exception>
	// Token: 0x06002A80 RID: 10880 RVA: 0x00095224 File Offset: 0x00093424
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

	/// <summary>Sets the date and time, in coordinated universal time (UTC), that the specified file was last accessed.</summary>
	/// <param name="path">The file for which to set the access date and time information.</param>
	/// <param name="lastAccessTimeUtc">A <see cref="T:System.DateTime" /> containing the value to set for the last access date and time of <paramref name="path" />. This value is expressed in UTC time.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="lastAccessTimeUtc" /> specifies a value outside the range of dates or times permitted for this operation.</exception>
	// Token: 0x06002A81 RID: 10881 RVA: 0x0009525C File Offset: 0x0009345C
	public static void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
	{
		File.SetLastAccessTime(path, lastAccessTimeUtc.ToLocalTime());
	}

	/// <summary>Sets the date and time that the specified file was last written to.</summary>
	/// <param name="path">The file for which to set the date and time information.</param>
	/// <param name="lastWriteTime">A <see cref="T:System.DateTime" /> containing the value to set for the last write date and time of <paramref name="path" />. This value is expressed in local time.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="lastWriteTime" /> specifies a value outside the range of dates or times permitted for this operation.</exception>
	// Token: 0x06002A82 RID: 10882 RVA: 0x0009526C File Offset: 0x0009346C
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

	/// <summary>Sets the date and time, in coordinated universal time (UTC), that the specified file was last written to.</summary>
	/// <param name="path">The file for which to set the date and time information.</param>
	/// <param name="lastWriteTimeUtc">A <see cref="T:System.DateTime" /> containing the value to set for the last write date and time of <paramref name="path" />. This value is expressed in UTC time.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="lastWriteTimeUtc" /> specifies a value outside the range of dates or times permitted for this operation.</exception>
	// Token: 0x06002A83 RID: 10883 RVA: 0x000952A4 File Offset: 0x000934A4
	public static void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
	{
		File.SetLastWriteTime(path, lastWriteTimeUtc.ToLocalTime());
	}

	/// <summary>Opens a binary file, reads the contents of the file into a byte array, and then closes the file.</summary>
	/// <param name="path">The file to open for reading.</param>
	/// <returns>A byte array containing the contents of the file.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">This operation is not supported on the current platform.  
	///  -or-  
	///  <paramref name="path" /> specified a directory.  
	///  -or-  
	///  The caller does not have the required permission.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	// Token: 0x06002A84 RID: 10884 RVA: 0x000952B4 File Offset: 0x000934B4
	public static byte[] ReadAllBytes(string path)
	{
		byte[] result;
		using (FileStream fileStream = File.OpenRead(path))
		{
			long length = fileStream.Length;
			if (length > 2147483647L)
			{
				throw new IOException("Reading more than 2GB with this call is not supported");
			}
			int num = 0;
			int i = (int)length;
			byte[] array = new byte[length];
			while (i > 0)
			{
				int num2 = fileStream.Read(array, num, i);
				if (num2 == 0)
				{
					throw new IOException("Unexpected end of stream");
				}
				num += num2;
				i -= num2;
			}
			result = array;
		}
		return result;
	}

	/// <summary>Opens a text file, reads all lines of the file, and then closes the file.</summary>
	/// <param name="path">The file to open for reading.</param>
	/// <returns>A string array containing all lines of the file.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.  
	/// -or-  
	/// This operation is not supported on the current platform.  
	/// -or-  
	/// <paramref name="path" /> specified a directory.  
	/// -or-  
	/// The caller does not have the required permission.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	// Token: 0x06002A85 RID: 10885 RVA: 0x0009533C File Offset: 0x0009353C
	public static string[] ReadAllLines(string path)
	{
		string[] result;
		using (StreamReader streamReader = File.OpenText(path))
		{
			result = File.ReadAllLines(streamReader);
		}
		return result;
	}

	/// <summary>Opens a file, reads all lines of the file with the specified encoding, and then closes the file.</summary>
	/// <param name="path">The file to open for reading.</param>
	/// <param name="encoding">The encoding applied to the contents of the file.</param>
	/// <returns>A string array containing all lines of the file.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.  
	/// -or-  
	/// This operation is not supported on the current platform.  
	/// -or-  
	/// <paramref name="path" /> specified a directory.  
	/// -or-  
	/// The caller does not have the required permission.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	// Token: 0x06002A86 RID: 10886 RVA: 0x00095374 File Offset: 0x00093574
	public static string[] ReadAllLines(string path, Encoding encoding)
	{
		string[] result;
		using (StreamReader streamReader = new StreamReader(path, encoding))
		{
			result = File.ReadAllLines(streamReader);
		}
		return result;
	}

	// Token: 0x06002A87 RID: 10887 RVA: 0x000953B0 File Offset: 0x000935B0
	private static string[] ReadAllLines(StreamReader reader)
	{
		List<string> list = new List<string>();
		while (!reader.EndOfStream)
		{
			list.Add(reader.ReadLine());
		}
		return list.ToArray();
	}

	/// <summary>Opens a text file, reads all the text in the file, and then closes the file.</summary>
	/// <param name="path">The file to open for reading.</param>
	/// <returns>A string containing all the text in the file.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.  
	/// -or-  
	/// This operation is not supported on the current platform.  
	/// -or-  
	/// <paramref name="path" /> specified a directory.  
	/// -or-  
	/// The caller does not have the required permission.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	// Token: 0x06002A88 RID: 10888 RVA: 0x000953E0 File Offset: 0x000935E0
	public static string ReadAllText(string path)
	{
		string result;
		using (StreamReader streamReader = new StreamReader(path))
		{
			result = streamReader.ReadToEnd();
		}
		return result;
	}

	/// <summary>Opens a file, reads all text in the file with the specified encoding, and then closes the file.</summary>
	/// <param name="path">The file to open for reading.</param>
	/// <param name="encoding">The encoding applied to the contents of the file.</param>
	/// <returns>A string containing all text in the file.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.  
	/// -or-  
	/// This operation is not supported on the current platform.  
	/// -or-  
	/// <paramref name="path" /> specified a directory.  
	/// -or-  
	/// The caller does not have the required permission.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	// Token: 0x06002A89 RID: 10889 RVA: 0x00095418 File Offset: 0x00093618
	public static string ReadAllText(string path, Encoding encoding)
	{
		string result;
		using (StreamReader streamReader = new StreamReader(path, encoding))
		{
			result = streamReader.ReadToEnd();
		}
		return result;
	}

	/// <summary>Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.</summary>
	/// <param name="path">The file to write to.</param>
	/// <param name="bytes">The bytes to write to the file.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" /> or the byte array is empty.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.  
	/// -or-  
	/// This operation is not supported on the current platform.  
	/// -or-  
	/// <paramref name="path" /> specified a directory.  
	/// -or-  
	/// The caller does not have the required permission.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	// Token: 0x06002A8A RID: 10890 RVA: 0x00095454 File Offset: 0x00093654
	public static void WriteAllBytes(string path, byte[] bytes)
	{
		using (Stream stream = File.Create(path))
		{
			stream.Write(bytes, 0, bytes.Length);
		}
	}

	/// <summary>Creates a new file, write the specified string array to the file, and then closes the file.</summary>
	/// <param name="path">The file to write to.</param>
	/// <param name="contents">The string array to write to the file.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">Either <paramref name="path" /> or <paramref name="contents" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.  
	/// -or-  
	/// This operation is not supported on the current platform.  
	/// -or-  
	/// <paramref name="path" /> specified a directory.  
	/// -or-  
	/// The caller does not have the required permission.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	// Token: 0x06002A8B RID: 10891 RVA: 0x00095490 File Offset: 0x00093690
	public static void WriteAllLines(string path, string[] contents)
	{
		using (StreamWriter streamWriter = new StreamWriter(path))
		{
			File.WriteAllLines(streamWriter, contents);
		}
	}

	/// <summary>Creates a new file, writes the specified string array to the file by using the specified encoding, and then closes the file.</summary>
	/// <param name="path">The file to write to.</param>
	/// <param name="contents">The string array to write to the file.</param>
	/// <param name="encoding">An <see cref="T:System.Text.Encoding" /> object that represents the character encoding applied to the string array.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">Either <paramref name="path" /> or <paramref name="contents" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.  
	/// -or-  
	/// This operation is not supported on the current platform.  
	/// -or-  
	/// <paramref name="path" /> specified a directory.  
	/// -or-  
	/// The caller does not have the required permission.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	// Token: 0x06002A8C RID: 10892 RVA: 0x000954C8 File Offset: 0x000936C8
	public static void WriteAllLines(string path, string[] contents, Encoding encoding)
	{
		using (StreamWriter streamWriter = new StreamWriter(path, false, encoding))
		{
			File.WriteAllLines(streamWriter, contents);
		}
	}

	// Token: 0x06002A8D RID: 10893 RVA: 0x00095504 File Offset: 0x00093704
	private static void WriteAllLines(StreamWriter writer, string[] contents)
	{
		foreach (string value in contents)
		{
			writer.WriteLine(value);
		}
	}

	/// <summary>Creates a new file, writes the specified string to the file, and then closes the file. If the target file already exists, it is overwritten.</summary>
	/// <param name="path">The file to write to.</param>
	/// <param name="contents">The string to write to the file.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" /> or <paramref name="contents" /> is empty.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.  
	/// -or-  
	/// This operation is not supported on the current platform.  
	/// -or-  
	/// <paramref name="path" /> specified a directory.  
	/// -or-  
	/// The caller does not have the required permission.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	// Token: 0x06002A8E RID: 10894 RVA: 0x0009552C File Offset: 0x0009372C
	public static void WriteAllText(string path, string contents)
	{
		File.WriteAllText(path, contents, EncodingHelper.UTF8Unmarked);
	}

	/// <summary>Creates a new file, writes the specified string to the file using the specified encoding, and then closes the file. If the target file already exists, it is overwritten.</summary>
	/// <param name="path">The file to write to.</param>
	/// <param name="contents">The string to write to the file.</param>
	/// <param name="encoding">The encoding to apply to the string.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" /> or <paramref name="contents" /> is empty.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specified a file that is read-only.  
	/// -or-  
	/// This operation is not supported on the current platform.  
	/// -or-  
	/// <paramref name="path" /> specified a directory.  
	/// -or-  
	/// The caller does not have the required permission.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	// Token: 0x06002A8F RID: 10895 RVA: 0x0009553C File Offset: 0x0009373C
	public static void WriteAllText(string path, string contents, Encoding encoding)
	{
		using (StreamWriter streamWriter = new StreamWriter(path, false, encoding))
		{
			streamWriter.Write(contents);
		}
	}

	// Token: 0x17000675 RID: 1653
	// (get) Token: 0x06002A90 RID: 10896 RVA: 0x00095578 File Offset: 0x00093778
	private static DateTime DefaultLocalFileTime
	{
		get
		{
			if (File.defaultLocalFileTime == null)
			{
				File.defaultLocalFileTime = new DateTime?(new DateTime(1601, 1, 1).ToLocalTime());
			}
			return File.defaultLocalFileTime.Value;
		}
	}

	/// <summary>Encrypts a file so that only the account used to encrypt the file can decrypt it.</summary>
	/// <param name="path">A path that describes a file to encrypt.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="path" /> parameter is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> parameter is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.DriveNotFoundException">An invalid drive was specified.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file described by the <paramref name="path" /> parameter could not be found.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.  
	///  -or-  
	///  This operation is not supported on the current platform.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
	/// <exception cref="T:System.NotSupportedException">The file system is not NTFS.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="path" /> parameter specified a file that is read-only.  
	///  -or-  
	///  This operation is not supported on the current platform.  
	///  -or-  
	///  The <paramref name="path" /> parameter specified a directory.  
	///  -or-  
	///  The caller does not have the required permission.</exception>
	// Token: 0x06002A91 RID: 10897 RVA: 0x000955B9 File Offset: 0x000937B9
	[MonoLimitation("File encryption isn't supported (even on NTFS).")]
	public static void Encrypt(string path)
	{
		throw new NotSupportedException(Locale.GetText("File encryption isn't supported on any file system."));
	}

	/// <summary>Decrypts a file that was encrypted by the current account using the <see cref="M:System.IO.File.Encrypt(System.String)" /> method.</summary>
	/// <param name="path">A path that describes a file to decrypt.</param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="path" /> parameter is a zero-length string, contains only white space, or contains one or more invalid characters as defined by <see cref="F:System.IO.Path.InvalidPathChars" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> parameter is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.DriveNotFoundException">An invalid drive was specified.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file described by the <paramref name="path" /> parameter could not be found.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file. For example, the encrypted file is already open.  
	///  -or-  
	///  This operation is not supported on the current platform.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
	/// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
	/// <exception cref="T:System.NotSupportedException">The file system is not NTFS.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="path" /> parameter specified a file that is read-only.  
	///  -or-  
	///  This operation is not supported on the current platform.  
	///  -or-  
	///  The <paramref name="path" /> parameter specified a directory.  
	///  -or-  
	///  The caller does not have the required permission.</exception>
	// Token: 0x06002A92 RID: 10898 RVA: 0x000955B9 File Offset: 0x000937B9
	[MonoLimitation("File encryption isn't supported (even on NTFS).")]
	public static void Decrypt(string path)
	{
		throw new NotSupportedException(Locale.GetText("File encryption isn't supported on any file system."));
	}

	/// <summary>Reads the lines of a file.</summary>
	/// <param name="path">The file to read.</param>
	/// <returns>All the lines of the file, or the lines that are the result of a query.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the system-defined maximum length.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specifies a file that is read-only.  
	/// -or-  
	/// This operation is not supported on the current platform.  
	/// -or-  
	/// <paramref name="path" /> is a directory.  
	/// -or-  
	/// The caller does not have the required permission.</exception>
	// Token: 0x06002A93 RID: 10899 RVA: 0x000955CA File Offset: 0x000937CA
	public static IEnumerable<string> ReadLines(string path)
	{
		return File.ReadLines(File.OpenText(path));
	}

	/// <summary>Read the lines of a file that has a specified encoding.</summary>
	/// <param name="path">The file to read.</param>
	/// <param name="encoding">The encoding that is applied to the contents of the file.</param>
	/// <returns>All the lines of the file, or the lines that are the result of a query.</returns>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="path" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the system-defined maximum length.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specifies a file that is read-only.  
	/// -or-  
	/// This operation is not supported on the current platform.  
	/// -or-  
	/// <paramref name="path" /> is a directory.  
	/// -or-  
	/// The caller does not have the required permission.</exception>
	// Token: 0x06002A94 RID: 10900 RVA: 0x000955D7 File Offset: 0x000937D7
	public static IEnumerable<string> ReadLines(string path, Encoding encoding)
	{
		return File.ReadLines(new StreamReader(path, encoding));
	}

	// Token: 0x06002A95 RID: 10901 RVA: 0x000955E5 File Offset: 0x000937E5
	private static IEnumerable<string> ReadLines(StreamReader reader)
	{
		using (reader)
		{
			string s;
			while ((s = reader.ReadLine()) != null)
			{
				yield return s;
			}
			s = null;
		}
		StreamReader streamReader = null;
		yield break;
		yield break;
	}

	/// <summary>Appends lines to a file, and then closes the file. If the specified file does not exist, this method creates a file, writes the specified lines to the file, and then closes the file.</summary>
	/// <param name="path">The file to append the lines to. The file is created if it doesn't already exist.</param>
	/// <param name="contents">The lines to append to the file.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one more invalid characters defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
	/// <exception cref="T:System.ArgumentNullException">Either <paramref name="path" /> or <paramref name="contents" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> is invalid (for example, the directory doesn't exist or it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the system-defined maximum length.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have permission to write to the file.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specifies a file that is read-only.  
	/// -or-  
	/// This operation is not supported on the current platform.  
	/// -or-  
	/// <paramref name="path" /> is a directory.</exception>
	// Token: 0x06002A96 RID: 10902 RVA: 0x000955F8 File Offset: 0x000937F8
	public static void AppendAllLines(string path, IEnumerable<string> contents)
	{
		Path.Validate(path);
		if (contents == null)
		{
			return;
		}
		using (TextWriter textWriter = new StreamWriter(path, true))
		{
			foreach (string value in contents)
			{
				textWriter.WriteLine(value);
			}
		}
	}

	/// <summary>Appends lines to a file by using a specified encoding, and then closes the file. If the specified file does not exist, this method creates a file, writes the specified lines to the file, and then closes the file.</summary>
	/// <param name="path">The file to append the lines to. The file is created if it doesn't already exist.</param>
	/// <param name="contents">The lines to append to the file.</param>
	/// <param name="encoding">The character encoding to use.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one more invalid characters defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
	/// <exception cref="T:System.ArgumentNullException">Either <paramref name="path" />, <paramref name="contents" />, or <paramref name="encoding" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> is invalid (for example, the directory doesn't exist or it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The file specified by <paramref name="path" /> was not found.</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the system-defined maximum length.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specifies a file that is read-only.  
	/// -or-  
	/// This operation is not supported on the current platform.  
	/// -or-  
	/// <paramref name="path" /> is a directory.  
	/// -or-  
	/// The caller does not have the required permission.</exception>
	// Token: 0x06002A97 RID: 10903 RVA: 0x00095668 File Offset: 0x00093868
	public static void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
	{
		Path.Validate(path);
		if (contents == null)
		{
			return;
		}
		using (TextWriter textWriter = new StreamWriter(path, true, encoding))
		{
			foreach (string value in contents)
			{
				textWriter.WriteLine(value);
			}
		}
	}

	/// <summary>Creates a new file, writes a collection of strings to the file, and then closes the file.</summary>
	/// <param name="path">The file to write to.</param>
	/// <param name="contents">The lines to write to the file.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
	/// <exception cref="T:System.ArgumentNullException">Either <paramref name="path" /> or <paramref name="contents" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the system-defined maximum length.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specifies a file that is read-only.  
	/// -or-  
	/// This operation is not supported on the current platform.  
	/// -or-  
	/// <paramref name="path" /> is a directory.  
	/// -or-  
	/// The caller does not have the required permission.</exception>
	// Token: 0x06002A98 RID: 10904 RVA: 0x000956DC File Offset: 0x000938DC
	public static void WriteAllLines(string path, IEnumerable<string> contents)
	{
		Path.Validate(path);
		if (contents == null)
		{
			return;
		}
		using (TextWriter textWriter = new StreamWriter(path, false))
		{
			foreach (string value in contents)
			{
				textWriter.WriteLine(value);
			}
		}
	}

	/// <summary>Creates a new file by using the specified encoding, writes a collection of strings to the file, and then closes the file.</summary>
	/// <param name="path">The file to write to.</param>
	/// <param name="contents">The lines to write to the file.</param>
	/// <param name="encoding">The character encoding to use.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters defined by the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
	/// <exception cref="T:System.ArgumentNullException">Either <paramref name="path" />, <paramref name="contents" />, or <paramref name="encoding" /> is <see langword="null" />.</exception>
	/// <exception cref="T:System.IO.DirectoryNotFoundException">
	///   <paramref name="path" /> is invalid (for example, it is on an unmapped drive).</exception>
	/// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the file.</exception>
	/// <exception cref="T:System.IO.PathTooLongException">
	///   <paramref name="path" /> exceeds the system-defined maximum length.</exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="path" /> is in an invalid format.</exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">
	///   <paramref name="path" /> specifies a file that is read-only.  
	/// -or-  
	/// This operation is not supported on the current platform.  
	/// -or-  
	/// <paramref name="path" /> is a directory.  
	/// -or-  
	/// The caller does not have the required permission.</exception>
	// Token: 0x06002A99 RID: 10905 RVA: 0x0009574C File Offset: 0x0009394C
	public static void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
	{
		Path.Validate(path);
		if (contents == null)
		{
			return;
		}
		using (TextWriter textWriter = new StreamWriter(path, false, encoding))
		{
			foreach (string value in contents)
			{
				textWriter.WriteLine(value);
			}
		}
	}

	// Token: 0x06002A9A RID: 10906 RVA: 0x000957C0 File Offset: 0x000939C0
	internal static int FillAttributeInfo(string path, ref MonoIOStat data, bool tryagain, bool returnErrorOnNotFound)
	{
		if (tryagain)
		{
			throw new NotImplementedException();
		}
		MonoIOError monoIOError;
		MonoIO.GetFileStat(path, out data, out monoIOError);
		if (!returnErrorOnNotFound && (monoIOError == MonoIOError.ERROR_FILE_NOT_FOUND || monoIOError == MonoIOError.ERROR_PATH_NOT_FOUND || monoIOError == MonoIOError.ERROR_NOT_READY))
		{
			data = default(MonoIOStat);
			data.fileAttributes = (FileAttributes)(-1);
			return 0;
		}
		return (int)monoIOError;
	}

	// Token: 0x04001647 RID: 5703
	private static DateTime? defaultLocalFileTime;
    */
}
