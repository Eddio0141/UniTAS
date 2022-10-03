using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UniTASPlugin.FakeGameState.GameFileSystem;

namespace UniTASPlugin.Patches.__System.__IO;

[HarmonyPatch(typeof(Directory), nameof(Directory.Exists))]
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

class Dummy
{
    /// <summary>Returns the names of files (including their paths) in the specified directory.</summary>
    /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
    /// <returns>An array of the full names (including paths) for the files in the specified directory, or an empty array if no files are found.</returns>
    /// <exception cref="T:System.IO.IOException">
    ///   <paramref name="path" /> is a file name.  
    /// -or-  
    /// A network error has occurred.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is not found or is invalid (for example, it is on an unmapped drive).</exception>
    // Token: 0x060026C2 RID: 9922 RVA: 0x0008853A File Offset: 0x0008673A
    /*
    public static string[] GetFiles(string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        return Directory.InternalGetFiles(path, "*", SearchOption.TopDirectoryOnly);
    }

    /// <summary>Returns the names of files (including their paths) that match the specified search pattern in the specified directory.</summary>
    /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
    /// <param name="searchPattern">The search string to match against the names of files in <paramref name="path" />.  This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
    /// <returns>An array of the full names (including paths) for the files in the specified directory that match the specified search pattern, or an empty array if no files are found.</returns>
    /// <exception cref="T:System.IO.IOException">
    ///   <paramref name="path" /> is a file name.  
    /// -or-  
    /// A network error has occurred.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters by using <see cref="M:System.IO.Path.GetInvalidPathChars" />.  
    /// -or-  
    /// <paramref name="searchPattern" /> doesn't contain a valid pattern.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> or <paramref name="searchPattern" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is not found or is invalid (for example, it is on an unmapped drive).</exception>
    // Token: 0x060026C3 RID: 9923 RVA: 0x00088556 File Offset: 0x00086756
    public static string[] GetFiles(string path, string searchPattern)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (searchPattern == null)
        {
            throw new ArgumentNullException("searchPattern");
        }
        return Directory.InternalGetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
    }

    /// <summary>Returns the names of files (including their paths) that match the specified search pattern in the specified directory, using a value to determine whether to search subdirectories.</summary>
    /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
    /// <param name="searchPattern">The search string to match against the names of files in <paramref name="path" />.  This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
    /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include all subdirectories or only the current directory.</param>
    /// <returns>An array of the full names (including paths) for the files in the specified directory that match the specified search pattern and option, or an empty array if no files are found.</returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters with the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.  
    /// -or-  
    /// <paramref name="searchPattern" /> does not contain a valid pattern.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> or <paramref name="searchpattern" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="searchOption" /> is not a valid <see cref="T:System.IO.SearchOption" /> value.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is not found or is invalid (for example, it is on an unmapped drive).</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.IO.IOException">
    ///   <paramref name="path" /> is a file name.  
    /// -or-  
    /// A network error has occurred.</exception>
    // Token: 0x060026C4 RID: 9924 RVA: 0x0008857C File Offset: 0x0008677C
    public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (searchPattern == null)
        {
            throw new ArgumentNullException("searchPattern");
        }
        if (searchOption != SearchOption.TopDirectoryOnly && searchOption != SearchOption.AllDirectories)
        {
            throw new ArgumentOutOfRangeException("searchOption", Environment.GetResourceString("Enum value was out of legal range."));
        }
        return Directory.InternalGetFiles(path, searchPattern, searchOption);
    }

    // Token: 0x060026C5 RID: 9925 RVA: 0x000885C9 File Offset: 0x000867C9
    private static string[] InternalGetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.InternalGetFileDirectoryNames(path, path, searchPattern, true, false, searchOption, true);
    }

    // Token: 0x060026C6 RID: 9926 RVA: 0x000885D7 File Offset: 0x000867D7
    [SecurityCritical]
    internal static string[] UnsafeGetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.InternalGetFileDirectoryNames(path, path, searchPattern, true, false, searchOption, false);
    }

    /// <summary>Returns the names of subdirectories (including their paths) in the specified directory.</summary>
    /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
    /// <returns>An array of the full names (including paths) of subdirectories in the specified path, or an empty array if no directories are found.</returns>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.IO.IOException">
    ///   <paramref name="path" /> is a file name.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
    // Token: 0x060026C7 RID: 9927 RVA: 0x000885E5 File Offset: 0x000867E5
    public static string[] GetDirectories(string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        return Directory.InternalGetDirectories(path, "*", SearchOption.TopDirectoryOnly);
    }

    /// <summary>Returns the names of subdirectories (including their paths) that match the specified search pattern in the specified directory.</summary>
    /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
    /// <param name="searchPattern">The search string to match against the names of subdirectories in <paramref name="path" />. This parameter can contain a combination of valid literal and wildcard characters, but it doesn't support regular expressions.</param>
    /// <returns>An array of the full names (including paths) of the subdirectories that match the search pattern in the specified directory, or an empty array if no directories are found.</returns>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters by using <see cref="M:System.IO.Path.GetInvalidPathChars" />.  
    /// -or-  
    /// <paramref name="searchPattern" /> doesn't contain a valid pattern.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> or <paramref name="searchPattern" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.IO.IOException">
    ///   <paramref name="path" /> is a file name.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
    // Token: 0x060026C8 RID: 9928 RVA: 0x00088601 File Offset: 0x00086801
    public static string[] GetDirectories(string path, string searchPattern)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (searchPattern == null)
        {
            throw new ArgumentNullException("searchPattern");
        }
        return Directory.InternalGetDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
    }

    /// <summary>Returns the names of the subdirectories (including their paths) that match the specified search pattern in the specified directory, and optionally searches subdirectories.</summary>
    /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
    /// <param name="searchPattern">The search string to match against the names of subdirectories in <paramref name="path" />. This parameter can contain a combination of valid literal and wildcard characters, but it doesn't support regular expressions.</param>
    /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include all subdirectories or only the current directory.</param>
    /// <returns>An array of the full names (including paths) of the subdirectories that match the specified criteria, or an empty array if no directories are found.</returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.  
    /// -or-  
    /// <paramref name="searchPattern" /> does not contain a valid pattern.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> or <paramref name="searchPattern" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="searchOption" /> is not a valid <see cref="T:System.IO.SearchOption" /> value.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.IO.IOException">
    ///   <paramref name="path" /> is a file name.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
    // Token: 0x060026C9 RID: 9929 RVA: 0x00088628 File Offset: 0x00086828
    public static string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (searchPattern == null)
        {
            throw new ArgumentNullException("searchPattern");
        }
        if (searchOption != SearchOption.TopDirectoryOnly && searchOption != SearchOption.AllDirectories)
        {
            throw new ArgumentOutOfRangeException("searchOption", Environment.GetResourceString("Enum value was out of legal range."));
        }
        return Directory.InternalGetDirectories(path, searchPattern, searchOption);
    }

    // Token: 0x060026CA RID: 9930 RVA: 0x00088675 File Offset: 0x00086875
    private static string[] InternalGetDirectories(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.InternalGetFileDirectoryNames(path, path, searchPattern, false, true, searchOption, true);
    }

    // Token: 0x060026CB RID: 9931 RVA: 0x00088683 File Offset: 0x00086883
    [SecurityCritical]
    internal static string[] UnsafeGetDirectories(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.InternalGetFileDirectoryNames(path, path, searchPattern, false, true, searchOption, false);
    }

    /// <summary>Returns the names of all files and subdirectories in a specified path.</summary>
    /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
    /// <returns>An array of the names of files and subdirectories in the specified directory, or an empty array if no files or subdirectories are found.</returns>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters with <see cref="M:System.IO.Path.GetInvalidPathChars" />.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.IO.IOException">
    ///   <paramref name="path" /> is a file name.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
    // Token: 0x060026CC RID: 9932 RVA: 0x00088691 File Offset: 0x00086891
    public static string[] GetFileSystemEntries(string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        return Directory.InternalGetFileSystemEntries(path, "*", SearchOption.TopDirectoryOnly);
    }

    /// <summary>Returns an array of file names and directory names that match a search pattern in a specified path.</summary>
    /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
    /// <param name="searchPattern">The search string to match against the names of file and directories in <paramref name="path" />.  This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
    /// <returns>An array of file names and directory names that match the specified search criteria, or an empty array if no files or directories are found.</returns>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters with the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.  
    /// -or-  
    /// <paramref name="searchPattern" /> does not contain a valid pattern.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> or <paramref name="searchPattern" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.IO.IOException">
    ///   <paramref name="path" /> is a file name.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
    // Token: 0x060026CD RID: 9933 RVA: 0x000886AD File Offset: 0x000868AD
    public static string[] GetFileSystemEntries(string path, string searchPattern)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (searchPattern == null)
        {
            throw new ArgumentNullException("searchPattern");
        }
        return Directory.InternalGetFileSystemEntries(path, searchPattern, SearchOption.TopDirectoryOnly);
    }

    /// <summary>Returns an array of all the file names and directory names that match a search pattern in a specified path, and optionally searches subdirectories.</summary>
    /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
    /// <param name="searchPattern">The search string to match against the names of files and directories in <paramref name="path" />.  This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
    /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories.  
    ///  The default value is <see cref="F:System.IO.SearchOption.TopDirectoryOnly" />.</param>
    /// <returns>An array of file the file names and directory names that match the specified search criteria, or an empty array if no files or directories are found.</returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains invalid characters. You can query for invalid characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.  
    /// -or-
    ///  <paramref name="searchPattern" /> does not contain a valid pattern.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.  
    /// -or-  
    /// <paramref name="searchPattern" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="searchOption" /> is not a valid <see cref="T:System.IO.SearchOption" /> value.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">
    ///   <paramref name="path" /> is invalid, such as referring to an unmapped drive.</exception>
    /// <exception cref="T:System.IO.IOException">
    ///   <paramref name="path" /> is a file name.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or combined exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    // Token: 0x060026CE RID: 9934 RVA: 0x000886D4 File Offset: 0x000868D4
    public static string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (searchPattern == null)
        {
            throw new ArgumentNullException("searchPattern");
        }
        if (searchOption != SearchOption.TopDirectoryOnly && searchOption != SearchOption.AllDirectories)
        {
            throw new ArgumentOutOfRangeException("searchOption", Environment.GetResourceString("Enum value was out of legal range."));
        }
        return Directory.InternalGetFileSystemEntries(path, searchPattern, searchOption);
    }

    // Token: 0x060026CF RID: 9935 RVA: 0x00088721 File Offset: 0x00086921
    private static string[] InternalGetFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.InternalGetFileDirectoryNames(path, path, searchPattern, true, true, searchOption, true);
    }

    // Token: 0x060026D0 RID: 9936 RVA: 0x0008872F File Offset: 0x0008692F
    internal static string[] InternalGetFileDirectoryNames(string path, string userPathOriginal, string searchPattern, bool includeFiles, bool includeDirs, SearchOption searchOption, bool checkHost)
    {
        return new List<string>(FileSystemEnumerableFactory.CreateFileNameIterator(path, userPathOriginal, searchPattern, includeFiles, includeDirs, searchOption, checkHost)).ToArray();
    }

    /// <summary>Returns an enumerable collection of directory names in a specified path.</summary>
    /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
    /// <returns>An enumerable collection of the full names (including paths) for the directories in the directory specified by <paramref name="path" />.</returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains invalid characters. You can query for invalid characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">
    ///   <paramref name="path" /> is invalid, such as referring to an unmapped drive.</exception>
    /// <exception cref="T:System.IO.IOException">
    ///   <paramref name="path" /> is a file name.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or combined exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    // Token: 0x060026D1 RID: 9937 RVA: 0x0008874A File Offset: 0x0008694A
    public static IEnumerable<string> EnumerateDirectories(string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        return Directory.InternalEnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly);
    }

    /// <summary>Returns an enumerable collection of directory names that match a search pattern in a specified path.</summary>
    /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
    /// <param name="searchPattern">The search string to match against the names of directories in <paramref name="path" />.  This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
    /// <returns>An enumerable collection of the full names (including paths) for the directories in the directory specified by <paramref name="path" /> and that match the specified search pattern.</returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains invalid characters. You can query for invalid characters with the  <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.  
    /// -or-
    ///  <paramref name="searchPattern" /> does not contain a valid pattern.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.  
    /// -or-  
    /// <paramref name="searchPattern" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">
    ///   <paramref name="path" /> is invalid, such as referring to an unmapped drive.</exception>
    /// <exception cref="T:System.IO.IOException">
    ///   <paramref name="path" /> is a file name.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or combined exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    // Token: 0x060026D2 RID: 9938 RVA: 0x00088766 File Offset: 0x00086966
    public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (searchPattern == null)
        {
            throw new ArgumentNullException("searchPattern");
        }
        return Directory.InternalEnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
    }

    /// <summary>Returns an enumerable collection of directory names that match a search pattern in a specified path, and optionally searches subdirectories.</summary>
    /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
    /// <param name="searchPattern">The search string to match against the names of directories in <paramref name="path" />.  This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
    /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories.  
    ///  The default value is <see cref="F:System.IO.SearchOption.TopDirectoryOnly" />.</param>
    /// <returns>An enumerable collection of the full names (including paths) for the directories in the directory specified by <paramref name="path" /> and that match the specified search pattern and option.</returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains invalid characters. You can query for invalid characters by using the  <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.  
    /// -or-
    ///  <paramref name="searchPattern" /> does not contain a valid pattern.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.  
    /// -or-  
    /// <paramref name="searchPattern" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="searchOption" /> is not a valid <see cref="T:System.IO.SearchOption" /> value.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">
    ///   <paramref name="path" /> is invalid, such as referring to an unmapped drive.</exception>
    /// <exception cref="T:System.IO.IOException">
    ///   <paramref name="path" /> is a file name.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or combined exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    // Token: 0x060026D3 RID: 9939 RVA: 0x0008878C File Offset: 0x0008698C
    public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (searchPattern == null)
        {
            throw new ArgumentNullException("searchPattern");
        }
        if (searchOption != SearchOption.TopDirectoryOnly && searchOption != SearchOption.AllDirectories)
        {
            throw new ArgumentOutOfRangeException("searchOption", Environment.GetResourceString("Enum value was out of legal range."));
        }
        return Directory.InternalEnumerateDirectories(path, searchPattern, searchOption);
    }

    // Token: 0x060026D4 RID: 9940 RVA: 0x000887D9 File Offset: 0x000869D9
    private static IEnumerable<string> InternalEnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.EnumerateFileSystemNames(path, searchPattern, searchOption, false, true);
    }

    /// <summary>Returns an enumerable collection of file names in a specified path.</summary>
    /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
    /// <returns>An enumerable collection of the full names (including paths) for the files in the directory specified by <paramref name="path" />.</returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains invalid characters. You can query for invalid characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">
    ///   <paramref name="path" /> is invalid, such as referring to an unmapped drive.</exception>
    /// <exception cref="T:System.IO.IOException">
    ///   <paramref name="path" /> is a file name.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or combined exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    // Token: 0x060026D5 RID: 9941 RVA: 0x000887E5 File Offset: 0x000869E5
    public static IEnumerable<string> EnumerateFiles(string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        return Directory.InternalEnumerateFiles(path, "*", SearchOption.TopDirectoryOnly);
    }

    /// <summary>Returns an enumerable collection of file names that match a search pattern in a specified path.</summary>
    /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
    /// <param name="searchPattern">The search string to match against the names of files in <paramref name="path" />.  This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
    /// <returns>An enumerable collection of the full names (including paths) for the files in the directory specified by <paramref name="path" /> and that match the specified search pattern.</returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains invalid characters. You can query for invalid characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.  
    /// -or-
    ///  <paramref name="searchPattern" /> does not contain a valid pattern.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.  
    /// -or-  
    /// <paramref name="searchPattern" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">
    ///   <paramref name="path" /> is invalid, such as referring to an unmapped drive.</exception>
    /// <exception cref="T:System.IO.IOException">
    ///   <paramref name="path" /> is a file name.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or combined exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    // Token: 0x060026D6 RID: 9942 RVA: 0x00088801 File Offset: 0x00086A01
    public static IEnumerable<string> EnumerateFiles(string path, string searchPattern)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (searchPattern == null)
        {
            throw new ArgumentNullException("searchPattern");
        }
        return Directory.InternalEnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
    }

    /// <summary>Returns an enumerable collection of file names that match a search pattern in a specified path, and optionally searches subdirectories.</summary>
    /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
    /// <param name="searchPattern">The search string to match against the names of files in <paramref name="path" />.  This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
    /// <param name="searchOption">One of the enumeration values that specifies whether the search operation should include only the current directory or should include all subdirectories.  
    ///  The default value is <see cref="F:System.IO.SearchOption.TopDirectoryOnly" />.</param>
    /// <returns>An enumerable collection of the full names (including paths) for the files in the directory specified by <paramref name="path" /> and that match the specified search pattern and option.</returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains invalid characters. You can query for invalid characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.  
    /// -or-
    ///  <paramref name="searchPattern" /> does not contain a valid pattern.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.  
    /// -or-  
    /// <paramref name="searchPattern" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="searchOption" /> is not a valid <see cref="T:System.IO.SearchOption" /> value.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">
    ///   <paramref name="path" /> is invalid, such as referring to an unmapped drive.</exception>
    /// <exception cref="T:System.IO.IOException">
    ///   <paramref name="path" /> is a file name.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or combined exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    // Token: 0x060026D7 RID: 9943 RVA: 0x00088828 File Offset: 0x00086A28
    public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (searchPattern == null)
        {
            throw new ArgumentNullException("searchPattern");
        }
        if (searchOption != SearchOption.TopDirectoryOnly && searchOption != SearchOption.AllDirectories)
        {
            throw new ArgumentOutOfRangeException("searchOption", Environment.GetResourceString("Enum value was out of legal range."));
        }
        return Directory.InternalEnumerateFiles(path, searchPattern, searchOption);
    }

    // Token: 0x060026D8 RID: 9944 RVA: 0x00088875 File Offset: 0x00086A75
    private static IEnumerable<string> InternalEnumerateFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.EnumerateFileSystemNames(path, searchPattern, searchOption, true, false);
    }

    /// <summary>Returns an enumerable collection of file names and directory names in a specified path.</summary>
    /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
    /// <returns>An enumerable collection of file-system entries in the directory specified by <paramref name="path" />.</returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains invalid characters. You can query for invalid characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">
    ///   <paramref name="path" /> is invalid, such as referring to an unmapped drive.</exception>
    /// <exception cref="T:System.IO.IOException">
    ///   <paramref name="path" /> is a file name.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or combined exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    // Token: 0x060026D9 RID: 9945 RVA: 0x00088881 File Offset: 0x00086A81
    public static IEnumerable<string> EnumerateFileSystemEntries(string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        return Directory.InternalEnumerateFileSystemEntries(path, "*", SearchOption.TopDirectoryOnly);
    }

    /// <summary>Returns an enumerable collection of file names and directory names that  match a search pattern in a specified path.</summary>
    /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
    /// <param name="searchPattern">The search string to match against the names of file-system entries in <paramref name="path" />.  This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
    /// <returns>An enumerable collection of file-system entries in the directory specified by <paramref name="path" /> and that match the specified search pattern.</returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains invalid characters. You can query for invalid characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.  
    /// -or-
    ///  <paramref name="searchPattern" /> does not contain a valid pattern.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.  
    /// -or-  
    /// <paramref name="searchPattern" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">
    ///   <paramref name="path" /> is invalid, such as referring to an unmapped drive.</exception>
    /// <exception cref="T:System.IO.IOException">
    ///   <paramref name="path" /> is a file name.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or combined exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    // Token: 0x060026DA RID: 9946 RVA: 0x0008889D File Offset: 0x00086A9D
    public static IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (searchPattern == null)
        {
            throw new ArgumentNullException("searchPattern");
        }
        return Directory.InternalEnumerateFileSystemEntries(path, searchPattern, SearchOption.TopDirectoryOnly);
    }

    /// <summary>Returns an enumerable collection of file names and directory names that match a search pattern in a specified path, and optionally searches subdirectories.</summary>
    /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
    /// <param name="searchPattern">The search string to match against file-system entries in <paramref name="path" />.  This parameter can contain a combination of valid literal path and wildcard (* and ?) characters, but it doesn't support regular expressions.</param>
    /// <param name="searchOption">One of the enumeration values  that specifies whether the search operation should include only the current directory or should include all subdirectories.  
    ///  The default value is <see cref="F:System.IO.SearchOption.TopDirectoryOnly" />.</param>
    /// <returns>An enumerable collection of file-system entries in the directory specified by <paramref name="path" /> and that match the specified search pattern and option.</returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains invalid characters. You can query for invalid characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.  
    /// -or-
    ///  <paramref name="searchPattern" /> does not contain a valid pattern.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.  
    /// -or-  
    /// <paramref name="searchPattern" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="searchOption" /> is not a valid <see cref="T:System.IO.SearchOption" /> value.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">
    ///   <paramref name="path" /> is invalid, such as referring to an unmapped drive.</exception>
    /// <exception cref="T:System.IO.IOException">
    ///   <paramref name="path" /> is a file name.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or combined exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    // Token: 0x060026DB RID: 9947 RVA: 0x000888C4 File Offset: 0x00086AC4
    public static IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (searchPattern == null)
        {
            throw new ArgumentNullException("searchPattern");
        }
        if (searchOption != SearchOption.TopDirectoryOnly && searchOption != SearchOption.AllDirectories)
        {
            throw new ArgumentOutOfRangeException("searchOption", Environment.GetResourceString("Enum value was out of legal range."));
        }
        return Directory.InternalEnumerateFileSystemEntries(path, searchPattern, searchOption);
    }

    // Token: 0x060026DC RID: 9948 RVA: 0x00088911 File Offset: 0x00086B11
    private static IEnumerable<string> InternalEnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.EnumerateFileSystemNames(path, searchPattern, searchOption, true, true);
    }

    // Token: 0x060026DD RID: 9949 RVA: 0x0008891D File Offset: 0x00086B1D
    private static IEnumerable<string> EnumerateFileSystemNames(string path, string searchPattern, SearchOption searchOption, bool includeFiles, bool includeDirs)
    {
        return FileSystemEnumerableFactory.CreateFileNameIterator(path, path, searchPattern, includeFiles, includeDirs, searchOption, true);
    }

    /// <summary>Returns the volume information, root information, or both for the specified path.</summary>
    /// <param name="path">The path of a file or directory.</param>
    /// <returns>A string that contains the volume information, root information, or both for the specified path.</returns>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters with <see cref="M:System.IO.Path.GetInvalidPathChars" />.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    // Token: 0x060026DE RID: 9950 RVA: 0x0008892C File Offset: 0x00086B2C
    public static string GetDirectoryRoot(string path)
    {
        Path.Validate(path);
        SecurityManager.EnsureElevatedPermissions();
        return new string(Path.DirectorySeparatorChar, 1);
    }

    /// <summary>Creates all directories and subdirectories in the specified path unless they already exist.</summary>
    /// <param name="path">The directory to create.</param>
    /// <returns>An object that represents the directory at the specified path. This object is returned regardless of whether a directory at the specified path already exists.</returns>
    /// <exception cref="T:System.IO.IOException">The directory specified by <paramref name="path" /> is a file.  
    ///  -or-  
    ///  The network name is not known.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.  
    /// -or-  
    /// <paramref name="path" /> is prefixed with, or contains, only a colon character (:).</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
    /// <exception cref="T:System.NotSupportedException">
    ///   <paramref name="path" /> contains a colon character (:) that is not part of a drive label ("C:\").</exception>
    // Token: 0x060026DF RID: 9951 RVA: 0x00088944 File Offset: 0x00086B44
    public static DirectoryInfo CreateDirectory(string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (path.Length == 0)
        {
            throw new ArgumentException("Path is empty");
        }
        if (path.IndexOfAny(Path.InvalidPathChars) != -1)
        {
            throw new ArgumentException("Path contains invalid chars");
        }
        if (path.Trim().Length == 0)
        {
            throw new ArgumentException("Only blank characters in path");
        }
        SecurityManager.EnsureElevatedPermissions();
        if (File.Exists(path))
        {
            throw new IOException("Cannot create " + path + " because a file with the same name already exists.");
        }
        if (Environment.IsRunningOnWindows && path == ":")
        {
            throw new ArgumentException("Only ':' In path");
        }
        return Directory.CreateDirectoriesInternal(path);
    }

    /// <summary>Creates all the directories in the specified path, unless the already exist, applying the specified Windows security.</summary>
    /// <param name="path">The directory to create.</param>
    /// <param name="directorySecurity">The access control to apply to the directory.</param>
    /// <returns>An object that represents the directory at the specified path. This object is returned regardless of whether a directory at the specified path already exists.</returns>
    /// <exception cref="T:System.IO.IOException">The directory specified by <paramref name="path" /> is a file.  
    ///  -or-  
    ///  The network name is not known.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters by using the  <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.  
    /// -or-  
    /// <paramref name="path" /> is prefixed with, or contains, only a colon character (:).</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
    /// <exception cref="T:System.NotSupportedException">
    ///   <paramref name="path" /> contains a colon character (:) that is not part of a drive label ("C:\").</exception>
    // Token: 0x060026E0 RID: 9952 RVA: 0x000889EB File Offset: 0x00086BEB
    [MonoLimitation("DirectorySecurity not implemented")]
    public static DirectoryInfo CreateDirectory(string path, DirectorySecurity directorySecurity)
    {
        return Directory.CreateDirectory(path);
    }

    // Token: 0x060026E1 RID: 9953 RVA: 0x000889F4 File Offset: 0x00086BF4
    private static DirectoryInfo CreateDirectoriesInternal(string path)
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
        return directoryInfo;
    }

    /// <summary>Deletes an empty directory from a specified path.</summary>
    /// <param name="path">The name of the empty directory to remove. This directory must be writable and empty.</param>
    /// <exception cref="T:System.IO.IOException">A file with the same name and location specified by <paramref name="path" /> exists.  
    ///  -or-  
    ///  The directory is the application's current working directory.  
    ///  -or-  
    ///  The directory specified by <paramref name="path" /> is not empty.  
    ///  -or-  
    ///  The directory is read-only or contains a read-only file.  
    ///  -or-  
    ///  The directory is being used by another process.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">
    ///   <paramref name="path" /> does not exist or could not be found.  
    /// -or-  
    /// The specified path is invalid (for example, it is on an unmapped drive).</exception>
    // Token: 0x060026E2 RID: 9954 RVA: 0x00088A50 File Offset: 0x00086C50
    public static void Delete(string path)
    {
        Path.Validate(path);
        if (Environment.IsRunningOnWindows && path == ":")
        {
            throw new NotSupportedException("Only ':' In path");
        }
        SecurityManager.EnsureElevatedPermissions();
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
            return;
        }
        if (monoIOError != MonoIOError.ERROR_FILE_NOT_FOUND)
        {
            throw MonoIO.GetException(path, monoIOError);
        }
        if (File.Exists(path))
        {
            throw new IOException("Directory does not exist, but a file of the same name exists.");
        }
        throw new DirectoryNotFoundException("Directory does not exist.");
    }

    // Token: 0x060026E3 RID: 9955 RVA: 0x00088AD4 File Offset: 0x00086CD4
    private static void RecursiveDelete(string path)
    {
        foreach (string path2 in Directory.GetDirectories(path))
        {
            MonoIOError monoIOError;
            if (MonoIO.ExistsSymlink(path2, out monoIOError))
            {
                MonoIO.DeleteFile(path2, out monoIOError);
            }
            else
            {
                Directory.RecursiveDelete(path2);
            }
        }
        string[] array = Directory.GetFiles(path);
        for (int i = 0; i < array.Length; i++)
        {
            File.Delete(array[i]);
        }
        Directory.Delete(path);
    }

    /// <summary>Deletes the specified directory and, if indicated, any subdirectories and files in the directory.</summary>
    /// <param name="path">The name of the directory to remove.</param>
    /// <param name="recursive">
    ///   <see langword="true" /> to remove directories, subdirectories, and files in <paramref name="path" />; otherwise, <see langword="false" />.</param>
    /// <exception cref="T:System.IO.IOException">A file with the same name and location specified by <paramref name="path" /> exists.  
    ///  -or-  
    ///  The directory specified by <paramref name="path" /> is read-only, or <paramref name="recursive" /> is <see langword="false" /> and <paramref name="path" /> is not an empty directory.  
    ///  -or-  
    ///  The directory is the application's current working directory.  
    ///  -or-  
    ///  The directory contains a read-only file.  
    ///  -or-  
    ///  The directory is being used by another process.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">
    ///   <paramref name="path" /> does not exist or could not be found.  
    /// -or-  
    /// The specified path is invalid (for example, it is on an unmapped drive).</exception>
    // Token: 0x060026E4 RID: 9956 RVA: 0x00088B38 File Offset: 0x00086D38
    public static void Delete(string path, bool recursive)
    {
        Path.Validate(path);
        SecurityManager.EnsureElevatedPermissions();
        if (recursive)
        {
            Directory.RecursiveDelete(path);
            return;
        }
        Directory.Delete(path);
    }

    /// <summary>Returns the date and time the specified file or directory was last accessed.</summary>
    /// <param name="path">The file or directory for which to obtain access date and time information.</param>
    /// <returns>A structure that is set to the date and time the specified file or directory was last accessed. This value is expressed in local time.</returns>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters with the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.NotSupportedException">The <paramref name="path" /> parameter is in an invalid format.</exception>
    // Token: 0x060026E6 RID: 9958 RVA: 0x00088B9C File Offset: 0x00086D9C
    public static DateTime GetLastAccessTime(string path)
    {
        return File.GetLastAccessTime(path);
    }

    /// <summary>Returns the date and time, in Coordinated Universal Time (UTC) format, that the specified file or directory was last accessed.</summary>
    /// <param name="path">The file or directory for which to obtain access date and time information.</param>
    /// <returns>A structure that is set to the date and time the specified file or directory was last accessed. This value is expressed in UTC time.</returns>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters with the  <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.NotSupportedException">The <paramref name="path" /> parameter is in an invalid format.</exception>
    // Token: 0x060026E7 RID: 9959 RVA: 0x00088BA4 File Offset: 0x00086DA4
    public static DateTime GetLastAccessTimeUtc(string path)
    {
        return Directory.GetLastAccessTime(path).ToUniversalTime();
    }

    /// <summary>Returns the date and time the specified file or directory was last written to.</summary>
    /// <param name="path">The file or directory for which to obtain modification date and time information.</param>
    /// <returns>A structure that is set to the date and time the specified file or directory was last written to. This value is expressed in local time.</returns>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters with the  <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    // Token: 0x060026E8 RID: 9960 RVA: 0x00088BBF File Offset: 0x00086DBF
    public static DateTime GetLastWriteTime(string path)
    {
        return File.GetLastWriteTime(path);
    }

    /// <summary>Returns the date and time, in Coordinated Universal Time (UTC) format, that the specified file or directory was last written to.</summary>
    /// <param name="path">The file or directory for which to obtain modification date and time information.</param>
    /// <returns>A structure that is set to the date and time the specified file or directory was last written to. This value is expressed in UTC time.</returns>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters with the  <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    // Token: 0x060026E9 RID: 9961 RVA: 0x00088BC8 File Offset: 0x00086DC8
    public static DateTime GetLastWriteTimeUtc(string path)
    {
        return Directory.GetLastWriteTime(path).ToUniversalTime();
    }

    /// <summary>Gets the creation date and time of a directory.</summary>
    /// <param name="path">The path of the directory.</param>
    /// <returns>A structure that is set to the creation date and time for the specified directory. This value is expressed in local time.</returns>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    // Token: 0x060026EA RID: 9962 RVA: 0x00088BE3 File Offset: 0x00086DE3
    public static DateTime GetCreationTime(string path)
    {
        return File.GetCreationTime(path);
    }

    /// <summary>Gets the creation date and time, in Coordinated Universal Time (UTC) format, of a directory.</summary>
    /// <param name="path">The path of the directory.</param>
    /// <returns>A structure that is set to the creation date and time for the specified directory. This value is expressed in UTC time.</returns>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters by using the <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    // Token: 0x060026EB RID: 9963 RVA: 0x00088BEC File Offset: 0x00086DEC
    public static DateTime GetCreationTimeUtc(string path)
    {
        return Directory.GetCreationTime(path).ToUniversalTime();
    }

    /// <summary>Gets the current working directory of the application.</summary>
    /// <returns>A string that contains the absolute path of the current working directory, and does not end with a backslash (\).</returns>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.NotSupportedException">The operating system is Windows CE, which does not have current directory functionality.  
    ///  This method is available in the .NET Compact Framework, but is not currently supported.</exception>
    // Token: 0x060026EC RID: 9964 RVA: 0x00088C07 File Offset: 0x00086E07
    public static string GetCurrentDirectory()
    {
        SecurityManager.EnsureElevatedPermissions();
        return Directory.InsecureGetCurrentDirectory();
    }

    // Token: 0x060026ED RID: 9965 RVA: 0x00088C14 File Offset: 0x00086E14
    internal static string InsecureGetCurrentDirectory()
    {
        MonoIOError monoIOError;
        string currentDirectory = MonoIO.GetCurrentDirectory(out monoIOError);
        if (monoIOError != MonoIOError.ERROR_SUCCESS)
        {
            throw MonoIO.GetException(monoIOError);
        }
        return currentDirectory;
    }

    /// <summary>Retrieves the names of the logical drives on this computer in the form "&lt;drive letter&gt;:\".</summary>
    /// <returns>The logical drives on this computer.</returns>
    /// <exception cref="T:System.IO.IOException">An I/O error occured (for example, a disk error).</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    // Token: 0x060026EE RID: 9966 RVA: 0x00088C32 File Offset: 0x00086E32
    public static string[] GetLogicalDrives()
    {
        return Environment.GetLogicalDrives();
    }

    // Token: 0x060026EF RID: 9967 RVA: 0x00088C39 File Offset: 0x00086E39
    private static bool IsRootDirectory(string path)
    {
        return (Path.DirectorySeparatorChar == '/' && path == "/") || (Path.DirectorySeparatorChar == '\\' && path.Length == 3 && path.EndsWith(":\\"));
    }

    /// <summary>Retrieves the parent directory of the specified path, including both absolute and relative paths.</summary>
    /// <param name="path">The path for which to retrieve the parent directory.</param>
    /// <returns>The parent directory, or <see langword="null" /> if <paramref name="path" /> is the root directory, including the root of a UNC server or share name.</returns>
    /// <exception cref="T:System.IO.IOException">The directory specified by <paramref name="path" /> is read-only.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters with the  <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For more information, see the <see cref="T:System.IO.PathTooLongException" /> topic.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path was not found.</exception>
    /// <exception cref="T:System.NotSupportedException">
    ///   <paramref name="path" /> is in an invalid format.</exception>
    /// <exception cref="T:System.Security.SecurityException">.NET Framework only: The caller does not have the required permissions.</exception>
    // Token: 0x060026F0 RID: 9968 RVA: 0x00088C78 File Offset: 0x00086E78
    public static DirectoryInfo GetParent(string path)
    {
        Path.Validate(path);
        if (Directory.IsRootDirectory(path))
        {
            return null;
        }
        string text = Path.GetDirectoryName(path);
        if (text.Length == 0)
        {
            text = Directory.GetCurrentDirectory();
        }
        return new DirectoryInfo(text);
    }

    /// <summary>Moves a file or a directory and its contents to a new location.</summary>
    /// <param name="sourceDirName">The path of the file or directory to move.</param>
    /// <param name="destDirName">The path to the new location for <paramref name="sourceDirName" />. If <paramref name="sourceDirName" /> is a file, then <paramref name="destDirName" /> must also be a file name.</param>
    /// <exception cref="T:System.IO.IOException">An attempt was made to move a directory to a different volume.  
    ///  -or-  
    ///  <paramref name="destDirName" /> already exists.  
    ///  -or-  
    ///  The <paramref name="sourceDirName" /> and <paramref name="destDirName" /> parameters refer to the same file or directory.  
    ///  -or-  
    ///  The directory or a file within it is being used by another process.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="sourceDirName" /> or <paramref name="destDirName" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters with the  <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="sourceDirName" /> or <paramref name="destDirName" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">The path specified by <paramref name="sourceDirName" /> is invalid (for example, it is on an unmapped drive).</exception>
    // Token: 0x060026F1 RID: 9969 RVA: 0x00088CB0 File Offset: 0x00086EB0
    public static void Move(string sourceDirName, string destDirName)
    {
        if (sourceDirName == null)
        {
            throw new ArgumentNullException("sourceDirName");
        }
        if (destDirName == null)
        {
            throw new ArgumentNullException("destDirName");
        }
        if (sourceDirName.Trim().Length == 0 || sourceDirName.IndexOfAny(Path.InvalidPathChars) != -1)
        {
            throw new ArgumentException("Invalid source directory name: " + sourceDirName, "sourceDirName");
        }
        if (destDirName.Trim().Length == 0 || destDirName.IndexOfAny(Path.InvalidPathChars) != -1)
        {
            throw new ArgumentException("Invalid target directory name: " + destDirName, "destDirName");
        }
        if (sourceDirName == destDirName)
        {
            throw new IOException("Source and destination path must be different.");
        }
        SecurityManager.EnsureElevatedPermissions();
        if (Directory.Exists(destDirName))
        {
            throw new IOException(destDirName + " already exists.");
        }
        if (!Directory.Exists(sourceDirName) && !File.Exists(sourceDirName))
        {
            throw new DirectoryNotFoundException(sourceDirName + " does not exist");
        }
        MonoIOError error;
        if (!MonoIO.MoveFile(sourceDirName, destDirName, out error))
        {
            throw MonoIO.GetException(error);
        }
    }

    /// <summary>Applies access control list (ACL) entries described by a <see cref="T:System.Security.AccessControl.DirectorySecurity" /> object to the specified directory.</summary>
    /// <param name="path">A directory to add or remove access control list (ACL) entries from.</param>
    /// <param name="directorySecurity">A <see cref="T:System.Security.AccessControl.DirectorySecurity" /> object that describes an ACL entry to apply to the directory described by the <paramref name="path" /> parameter.</param>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="directorySecurity" /> parameter is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">The directory could not be found.</exception>
    /// <exception cref="T:System.ArgumentException">The <paramref name="path" /> was invalid.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The current process does not have access to the directory specified by <paramref name="path" />.  
    ///  -or-  
    ///  The current process does not have sufficient privilege to set the ACL entry.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows 2000 or later.</exception>
    // Token: 0x060026F2 RID: 9970 RVA: 0x00088DA0 File Offset: 0x00086FA0
    public static void SetAccessControl(string path, DirectorySecurity directorySecurity)
    {
        if (directorySecurity == null)
        {
            throw new ArgumentNullException("directorySecurity");
        }
        directorySecurity.PersistModifications(path);
    }

    /// <summary>Sets the creation date and time for the specified file or directory.</summary>
    /// <param name="path">The file or directory for which to set the creation date and time information.</param>
    /// <param name="creationTime">The date and time the file or directory was last written to. This value is expressed in local time.</param>
    /// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters with the  <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="creationTime" /> specifies a value outside the range of dates or times permitted for this operation.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
    // Token: 0x060026F3 RID: 9971 RVA: 0x00088DB7 File Offset: 0x00086FB7
    public static void SetCreationTime(string path, DateTime creationTime)
    {
        File.SetCreationTime(path, creationTime);
    }

    /// <summary>Sets the creation date and time, in Coordinated Universal Time (UTC) format, for the specified file or directory.</summary>
    /// <param name="path">The file or directory for which to set the creation date and time information.</param>
    /// <param name="creationTimeUtc">The date and time the directory or file was created. This value is expressed in local time.</param>
    /// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters with the  <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="creationTime" /> specifies a value outside the range of dates or times permitted for this operation.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
    // Token: 0x060026F4 RID: 9972 RVA: 0x00088DC0 File Offset: 0x00086FC0
    public static void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
    {
        Directory.SetCreationTime(path, creationTimeUtc.ToLocalTime());
    }

    /// <summary>Sets the application's current working directory to the specified directory.</summary>
    /// <param name="path">The path to which the current working directory is set.</param>
    /// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters with the  <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission to access unmanaged code.</exception>
    /// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found.</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified directory was not found.</exception>
    // Token: 0x060026F5 RID: 9973 RVA: 0x00088DD0 File Offset: 0x00086FD0
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    public static void SetCurrentDirectory(string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (path.Trim().Length == 0)
        {
            throw new ArgumentException("path string must not be an empty string or whitespace string");
        }
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException("Directory \"" + path + "\" not found.");
        }
        MonoIOError monoIOError;
        MonoIO.SetCurrentDirectory(path, out monoIOError);
        if (monoIOError != MonoIOError.ERROR_SUCCESS)
        {
            throw MonoIO.GetException(path, monoIOError);
        }
    }

    /// <summary>Sets the date and time the specified file or directory was last accessed.</summary>
    /// <param name="path">The file or directory for which to set the access date and time information.</param>
    /// <param name="lastAccessTime">An object that contains the value to set for the access date and time of <paramref name="path" />. This value is expressed in local time.</param>
    /// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters with the  <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="lastAccessTime" /> specifies a value outside the range of dates or times permitted for this operation.</exception>
    // Token: 0x060026F6 RID: 9974 RVA: 0x00088E35 File Offset: 0x00087035
    public static void SetLastAccessTime(string path, DateTime lastAccessTime)
    {
        File.SetLastAccessTime(path, lastAccessTime);
    }

    /// <summary>Sets the date and time, in Coordinated Universal Time (UTC) format, that the specified file or directory was last accessed.</summary>
    /// <param name="path">The file or directory for which to set the access date and time information.</param>
    /// <param name="lastAccessTimeUtc">An object that  contains the value to set for the access date and time of <paramref name="path" />. This value is expressed in UTC time.</param>
    /// <exception cref="T:System.IO.FileNotFoundException">The specified path was not found.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters with the  <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="lastAccessTimeUtc" /> specifies a value outside the range of dates or times permitted for this operation.</exception>
    // Token: 0x060026F7 RID: 9975 RVA: 0x00088E3E File Offset: 0x0008703E
    public static void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
    {
        Directory.SetLastAccessTime(path, lastAccessTimeUtc.ToLocalTime());
    }

    /// <summary>Sets the date and time a directory was last written to.</summary>
    /// <param name="path">The path of the directory.</param>
    /// <param name="lastWriteTime">The date and time the directory was last written to. This value is expressed in local time.</param>
    /// <exception cref="T:System.IO.FileNotFoundException">
    ///   <paramref name="path" /> was not found (for example, the directory doesn't exist or it is on an unmapped drive).</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">
    ///   <paramref name="path" /> was not found (for example, the directory doesn't exist or it is on an unmapped drive).</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters with the  <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="lastWriteTime" /> specifies a value outside the range of dates or times permitted for this operation.</exception>
    // Token: 0x060026F8 RID: 9976 RVA: 0x00088E4D File Offset: 0x0008704D
    public static void SetLastWriteTime(string path, DateTime lastWriteTime)
    {
        File.SetLastWriteTime(path, lastWriteTime);
    }

    /// <summary>Sets the date and time, in Coordinated Universal Time (UTC) format, that a directory was last written to.</summary>
    /// <param name="path">The path of the directory.</param>
    /// <param name="lastWriteTimeUtc">The date and time the directory was last written to. This value is expressed in UTC time.</param>
    /// <exception cref="T:System.IO.FileNotFoundException">
    ///   <paramref name="path" /> was not found (for example, the directory doesn't exist or it is on an unmapped drive).</exception>
    /// <exception cref="T:System.IO.DirectoryNotFoundException">
    ///   <paramref name="path" /> was not found (for example, the directory doesn't exist or it is on an unmapped drive).</exception>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more invalid characters. You can query for invalid characters with the  <see cref="M:System.IO.Path.GetInvalidPathChars" /> method.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows NT or later.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="lastWriteTimeUtc" /> specifies a value outside the range of dates or times permitted for this operation.</exception>
    // Token: 0x060026F9 RID: 9977 RVA: 0x00088E56 File Offset: 0x00087056
    public static void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
    {
        Directory.SetLastWriteTime(path, lastWriteTimeUtc.ToLocalTime());
    }

    /// <summary>Gets a <see cref="T:System.Security.AccessControl.DirectorySecurity" /> object that encapsulates the specified type of access control list (ACL) entries for a specified directory.</summary>
    /// <param name="path">The path to a directory containing a <see cref="T:System.Security.AccessControl.DirectorySecurity" /> object that describes the file's access control list (ACL) information.</param>
    /// <param name="includeSections">One of the <see cref="T:System.Security.AccessControl.AccessControlSections" /> values that specifies the type of access control list (ACL) information to receive.</param>
    /// <returns>An object that encapsulates the access control rules for the file described by the <paramref name="path" /> parameter.</returns>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> parameter is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the directory.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows 2000 or later.</exception>
    /// <exception cref="T:System.SystemException">A system-level error occurred, such as the directory could not be found. The specific exception may be a subclass of <see cref="T:System.SystemException" />.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="path" /> parameter specified a directory that is read-only.  
    ///  -or-  
    ///  This operation is not supported on the current platform.  
    ///  -or-  
    ///  The caller does not have the required permission.</exception>
    // Token: 0x060026FA RID: 9978 RVA: 0x00088E65 File Offset: 0x00087065
    public static DirectorySecurity GetAccessControl(string path, AccessControlSections includeSections)
    {
        return new DirectorySecurity(path, includeSections);
    }

    /// <summary>Gets a <see cref="T:System.Security.AccessControl.DirectorySecurity" /> object that encapsulates the access control list (ACL) entries for a specified directory.</summary>
    /// <param name="path">The path to a directory containing a <see cref="T:System.Security.AccessControl.DirectorySecurity" /> object that describes the file's access control list (ACL) information.</param>
    /// <returns>An object that encapsulates the access control rules for the file described by the <paramref name="path" /> parameter.</returns>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="path" /> parameter is <see langword="null" />.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the directory.</exception>
    /// <exception cref="T:System.PlatformNotSupportedException">The current operating system is not Windows 2000 or later.</exception>
    /// <exception cref="T:System.SystemException">A system-level error occurred, such as the directory could not be found. The specific exception may be a subclass of <see cref="T:System.SystemException" />.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException">The <paramref name="path" /> parameter specified a directory that is read-only.  
    ///  -or-  
    ///  This operation is not supported on the current platform.  
    ///  -or-  
    ///  The caller does not have the required permission.</exception>
    // Token: 0x060026FB RID: 9979 RVA: 0x00088E6E File Offset: 0x0008706E
    public static DirectorySecurity GetAccessControl(string path)
    {
        return Directory.GetAccessControl(path, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
    }

    // Token: 0x060026FC RID: 9980 RVA: 0x00088E78 File Offset: 0x00087078
    internal static string GetDemandDir(string fullPath, bool thisDirOnly)
    {
        string result;
        if (thisDirOnly)
        {
            if (fullPath.EndsWith(Path.DirectorySeparatorChar) || fullPath.EndsWith(Path.AltDirectorySeparatorChar))
            {
                result = fullPath + ".";
            }
            else
            {
                result = fullPath + Path.DirectorySeparatorCharAsString + ".";
            }
        }
        else if (!fullPath.EndsWith(Path.DirectorySeparatorChar) && !fullPath.EndsWith(Path.AltDirectorySeparatorChar))
        {
            result = fullPath + Path.DirectorySeparatorCharAsString;
        }
        else
        {
            result = fullPath;
        }
        return result;
    }

    // Token: 0x02000363 RID: 867
    internal sealed class SearchData
    {
        // Token: 0x060026FD RID: 9981 RVA: 0x00088EEE File Offset: 0x000870EE
        public SearchData(string fullPath, string userPath, SearchOption searchOption)
        {
            this.fullPath = fullPath;
            this.userPath = userPath;
            this.searchOption = searchOption;
        }

        // Token: 0x04001505 RID: 5381
        public readonly string fullPath;

        // Token: 0x04001506 RID: 5382
        public readonly string userPath;

        // Token: 0x04001507 RID: 5383
        public readonly SearchOption searchOption;
    }
    */
}