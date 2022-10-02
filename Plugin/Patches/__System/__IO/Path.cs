using HarmonyLib;
using System;
using System.Reflection;
using UniTASPlugin.FakeGameState.GameFileSystem;
using PathOrig = System.IO.Path;

namespace UniTASPlugin.Patches.__System.__IO;

[HarmonyPatch(typeof(PathOrig), "findExtension")]
class findExtension
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref int __result, string path)
    {
        if (path != null)
        {
            int num = path.LastIndexOf('.');
            int num2 = path.LastIndexOfAny(FileSystem.ExternalHelpers.PathSeparatorChars);
            if (num > num2)
            {
                __result = num;
                return false;
            }
        }
        __result = -1;
        return false;
    }
}

[HarmonyPatch(typeof(PathOrig), nameof(PathOrig.Combine), new Type[] { typeof(string), typeof(string) })]
class Combine
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref string __result, string path1, string path2)
    {
        if (path1 == null)
        {
            throw new ArgumentNullException("path1");
        }
        if (path2 == null)
        {
            throw new ArgumentNullException("path2");
        }
        if (path1.Length == 0)
        {
            __result = path2;
            return false;
        }
        if (path2.Length == 0)
        {
            __result = path1;
            return false;
        }
        if (path1.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
        {
            throw new ArgumentException("Illegal characters in path.");
        }
        if (path2.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
        {
            throw new ArgumentException("Illegal characters in path.");
        }
        if (PathOrig.IsPathRooted(path2))
        {
            __result = path2;
            return false;
        }
        char c = path1[path1.Length - 1];
        if (c != FileSystem.ExternalHelpers.DirectorySeparatorChar && c != FileSystem.ExternalHelpers.AltDirectorySeparatorChar && c != FileSystem.ExternalHelpers.VolumeSeparatorChar)
        {
            __result = path1 + FileSystem.ExternalHelpers.DirectorySeparatorStr + path2;
            return false;
        }
        __result = path1 + path2;
        return false;
    }
}

[HarmonyPatch(typeof(PathOrig), nameof(PathOrig.IsPathRooted))]
class IsPathRooted
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref bool __result, string path)
    {
        if (path == null || path.Length == 0)
        {
            __result = false;
            return false;
        }
        if (path.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
        {
            throw new ArgumentException("Illegal characters in path.");
        }
        char c = path[0];
        __result =
            c == FileSystem.ExternalHelpers.DirectorySeparatorChar ||
            c == FileSystem.ExternalHelpers.AltDirectorySeparatorChar ||
            (!FileSystem.ExternalHelpers.dirEqualsVolume && path.Length > 1 && path[1] == FileSystem.ExternalHelpers.VolumeSeparatorChar);
        return false;
    }
}

class Dummy2
{
    /*
    internal static string CleanPath(string s)
    {
        int length = s.Length;
        int num = 0;
        int num2 = 0;
        int num3 = 0;
        char c = s[0];
        if (length > 2 && c == '\\' && s[1] == '\\')
        {
            num3 = 2;
        }
        if (length == 1 && (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar))
        {
            return s;
        }
        for (int i = num3; i < length; i++)
        {
            char c2 = s[i];
            if (c2 == Path.DirectorySeparatorChar || c2 == Path.AltDirectorySeparatorChar)
            {
                if (Path.DirectorySeparatorChar != Path.AltDirectorySeparatorChar && c2 == Path.AltDirectorySeparatorChar)
                {
                    num2++;
                }
                if (i + 1 == length)
                {
                    num++;
                }
                else
                {
                    c2 = s[i + 1];
                    if (c2 == Path.DirectorySeparatorChar || c2 == Path.AltDirectorySeparatorChar)
                    {
                        num++;
                    }
                }
            }
        }
        if (num == 0 && num2 == 0)
        {
            return s;
        }
        char[] array = new char[length - num];
        if (num3 != 0)
        {
            array[0] = '\\';
            array[1] = '\\';
        }
        int j = num3;
        int num4 = num3;
        while (j < length && num4 < array.Length)
        {
            char c3 = s[j];
            if (c3 != Path.DirectorySeparatorChar && c3 != Path.AltDirectorySeparatorChar)
            {
                array[num4++] = c3;
            }
            else if (num4 + 1 != array.Length)
            {
                array[num4++] = Path.DirectorySeparatorChar;
                while (j < length - 1)
                {
                    c3 = s[j + 1];
                    if (c3 != Path.DirectorySeparatorChar && c3 != Path.AltDirectorySeparatorChar)
                    {
                        break;
                    }
                    j++;
                }
            }
            j++;
        }
        return new string(array);
    }

    /// <summary>Returns the directory information for the specified path string.</summary>
    /// <param name="path">The path of a file or directory.</param>
    /// <returns>Directory information for <paramref name="path" />, or <see langword="null" /> if <paramref name="path" /> denotes a root directory or is null. Returns <see cref="F:System.String.Empty" /> if <paramref name="path" /> does not contain directory information.</returns>
    /// <exception cref="T:System.ArgumentException">The <paramref name="path" /> parameter contains invalid characters, is empty, or contains only white spaces.</exception>
    /// <exception cref="T:System.IO.PathTooLongException">In the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.IO.IOException" />, instead.  
    ///
    ///
    ///
    ///
    ///  The <paramref name="path" /> parameter is longer than the system-defined maximum length.</exception>
    public static string GetDirectoryName(string path)
    {
        if (path == string.Empty)
        {
            throw new ArgumentException("Invalid path");
        }
        if (path == null || Path.GetPathRoot(path) == path)
        {
            return null;
        }
        if (path.Trim().Length == 0)
        {
            throw new ArgumentException("Argument string consists of whitespace characters only.");
        }
        if (path.IndexOfAny(Path.InvalidPathChars) > -1)
        {
            throw new ArgumentException("Path contains invalid characters");
        }
        int num = path.LastIndexOfAny(Path.PathSeparatorChars);
        if (num == 0)
        {
            num++;
        }
        if (num <= 0)
        {
            return string.Empty;
        }
        string text = path.Substring(0, num);
        int length = text.Length;
        if (length >= 2 && Path.DirectorySeparatorChar == '\\' && text[length - 1] == Path.VolumeSeparatorChar)
        {
            return text + Path.DirectorySeparatorChar.ToString();
        }
        if (length == 1 && Path.DirectorySeparatorChar == '\\' && path.Length >= 2 && path[num] == Path.VolumeSeparatorChar)
        {
            return text + Path.VolumeSeparatorChar.ToString();
        }
        return Path.CleanPath(text);
    }

    /// <summary>Returns the extension of the specified path string.</summary>
    /// <param name="path">The path string from which to get the extension.</param>
    /// <returns>The extension of the specified path (including the period "."), or <see langword="null" />, or <see cref="F:System.String.Empty" />. If <paramref name="path" /> is <see langword="null" />, <see cref="M:System.IO.Path.GetExtension(System.String)" /> returns <see langword="null" />. If <paramref name="path" /> does not have extension information, <see cref="M:System.IO.Path.GetExtension(System.String)" /> returns <see cref="F:System.String.Empty" />.</returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> contains one or more of the invalid characters defined in <see cref="M:System.IO.Path.GetInvalidPathChars" />.</exception>
    public static string GetExtension(string path)
    {
        if (path == null)
        {
            return null;
        }
        if (path.IndexOfAny(Path.InvalidPathChars) != -1)
        {
            throw new ArgumentException("Illegal characters in path.");
        }
        int num = Path.findExtension(path);
        if (num > -1 && num < path.Length - 1)
        {
            return path.Substring(num);
        }
        return string.Empty;
    }

    /// <summary>Returns the file name of the specified path string without the extension.</summary>
    /// <param name="path">The path of the file.</param>
    /// <returns>The string returned by <see cref="M:System.IO.Path.GetFileName(System.String)" />, minus the last period (.) and all characters following it.</returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> contains one or more of the invalid characters defined in <see cref="M:System.IO.Path.GetInvalidPathChars" />.</exception>
    public static string GetFileNameWithoutExtension(string path)
    {
        return Path.ChangeExtension(Path.GetFileName(path), null);
    }

    /// <summary>Returns the absolute path for the specified path string.</summary>
    /// <param name="path">The file or directory for which to obtain absolute path information.</param>
    /// <returns>The fully qualified location of <paramref name="path" />, such as "C:\MyFile.txt".</returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> is a zero-length string, contains only white space, or contains one or more of the invalid characters defined in <see cref="M:System.IO.Path.GetInvalidPathChars" />.  
    /// -or-  
    /// The system could not retrieve the absolute path.</exception>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permissions.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.NotSupportedException">
    ///   <paramref name="path" /> contains a colon (":") that is not part of a volume identifier (for example, "c:\").</exception>
    /// <exception cref="T:System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
    public static string GetFullPath(string path)
    {
        string result = Path.InsecureGetFullPath(path);
        SecurityManager.EnsureElevatedPermissions();
        return result;
    }

    internal static string GetFullPathInternal(string path)
    {
        return Path.InsecureGetFullPath(path);
    }

    [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetFullPathName(string path, int numBufferChars, StringBuilder buffer, ref IntPtr lpFilePartOrNull);

    internal static string GetFullPathName(string path)
    {
        StringBuilder stringBuilder = new StringBuilder(260);
        IntPtr zero = IntPtr.Zero;
        int fullPathName = Path.GetFullPathName(path, 260, stringBuilder, ref zero);
        if (fullPathName == 0)
        {
            int lastWin32Error = Marshal.GetLastWin32Error();
            throw new IOException("Windows API call to GetFullPathName failed, Windows error code: " + lastWin32Error);
        }
        if (fullPathName > 260)
        {
            stringBuilder = new StringBuilder(fullPathName);
            Path.GetFullPathName(path, fullPathName, stringBuilder, ref zero);
        }
        return stringBuilder.ToString();
    }

    internal static string WindowsDriveAdjustment(string path)
    {
        if (path.Length < 2)
        {
            if (path.Length == 1 && (path[0] == '\\' || path[0] == '/'))
            {
                return Path.GetPathRoot(Directory.GetCurrentDirectory());
            }
            return path;
        }
        else
        {
            if (path[1] != ':' || !char.IsLetter(path[0]))
            {
                return path;
            }
            string text = Directory.InsecureGetCurrentDirectory();
            if (path.Length == 2)
            {
                if (text[0] == path[0])
                {
                    path = text;
                }
                else
                {
                    path = Path.GetFullPathName(path);
                }
            }
            else if (path[2] != Path.DirectorySeparatorChar && path[2] != Path.AltDirectorySeparatorChar)
            {
                if (text[0] == path[0])
                {
                    path = Path.Combine(text, path.Substring(2, path.Length - 2));
                }
                else
                {
                    path = Path.GetFullPathName(path);
                }
            }
            return path;
        }
    }

    internal static string InsecureGetFullPath(string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (path.Trim().Length == 0)
        {
            throw new ArgumentException(Locale.GetText("The specified path is not of a legal form (empty)."));
        }
        if (Environment.IsRunningOnWindows)
        {
            path = Path.WindowsDriveAdjustment(path);
        }
        char c = path[path.Length - 1];
        bool flag = true;
        if (path.Length >= 2 && Path.IsDirectorySeparator(path[0]) && Path.IsDirectorySeparator(path[1]))
        {
            if (path.Length == 2 || path.IndexOf(path[0], 2) < 0)
            {
                throw new ArgumentException("UNC paths should be of the form \\\\server\\share.");
            }
            if (path[0] != Path.DirectorySeparatorChar)
            {
                path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
        }
        else if (!Path.IsPathRooted(path))
        {
            if (!Environment.IsRunningOnWindows)
            {
                int num = 0;
                while ((num = path.IndexOf('.', num)) != -1 && ++num != path.Length && path[num] != Path.DirectorySeparatorChar && path[num] != Path.AltDirectorySeparatorChar)
                {
                }
                flag = (num > 0);
            }
            string text = Directory.InsecureGetCurrentDirectory();
            if (text[text.Length - 1] == Path.DirectorySeparatorChar)
            {
                path = text + path;
            }
            else
            {
                path = text + Path.DirectorySeparatorChar.ToString() + path;
            }
        }
        else if (Path.DirectorySeparatorChar == '\\' && path.Length >= 2 && Path.IsDirectorySeparator(path[0]) && !Path.IsDirectorySeparator(path[1]))
        {
            string text2 = Directory.InsecureGetCurrentDirectory();
            if (text2[1] == Path.VolumeSeparatorChar)
            {
                path = text2.Substring(0, 2) + path;
            }
            else
            {
                path = text2.Substring(0, text2.IndexOf('\\', text2.IndexOfUnchecked("\\\\", 0, text2.Length) + 1));
            }
        }
        if (flag)
        {
            path = Path.CanonicalizePath(path);
        }
        if (Path.IsDirectorySeparator(c) && path[path.Length - 1] != Path.DirectorySeparatorChar)
        {
            path += Path.DirectorySeparatorChar.ToString();
        }
        string text3;
        if (MonoIO.RemapPath(path, out text3))
        {
            path = text3;
        }
        return path;
    }

    internal static bool IsDirectorySeparator(char c)
    {
        return c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
    }

    /// <summary>Gets the root directory information of the specified path.</summary>
    /// <param name="path">The path from which to obtain root directory information.</param>
    /// <returns>The root directory of <paramref name="path" />, or <see langword="null" /> if <paramref name="path" /> is <see langword="null" />, or an empty string if <paramref name="path" /> does not contain root directory information.</returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> contains one or more of the invalid characters defined in <see cref="M:System.IO.Path.GetInvalidPathChars" />.  
    /// -or-  
    /// <see cref="F:System.String.Empty" /> was passed to <paramref name="path" />.</exception>
    public static string GetPathRoot(string path)
    {
        if (path == null)
        {
            return null;
        }
        if (path.Trim().Length == 0)
        {
            throw new ArgumentException("The specified path is not of a legal form.");
        }
        if (!Path.IsPathRooted(path))
        {
            return string.Empty;
        }
        if (Path.DirectorySeparatorChar == '/')
        {
            if (!Path.IsDirectorySeparator(path[0]))
            {
                return string.Empty;
            }
            return Path.DirectorySeparatorStr;
        }
        else
        {
            int num = 2;
            if (path.Length == 1 && Path.IsDirectorySeparator(path[0]))
            {
                return Path.DirectorySeparatorStr;
            }
            if (path.Length < 2)
            {
                return string.Empty;
            }
            if (Path.IsDirectorySeparator(path[0]) && Path.IsDirectorySeparator(path[1]))
            {
                while (num < path.Length && !Path.IsDirectorySeparator(path[num]))
                {
                    num++;
                }
                if (num < path.Length)
                {
                    num++;
                    while (num < path.Length && !Path.IsDirectorySeparator(path[num]))
                    {
                        num++;
                    }
                }
                return Path.DirectorySeparatorStr + Path.DirectorySeparatorStr + path.Substring(2, num - 2).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
            if (Path.IsDirectorySeparator(path[0]))
            {
                return Path.DirectorySeparatorStr;
            }
            if (path[1] == Path.VolumeSeparatorChar)
            {
                if (path.Length >= 3 && Path.IsDirectorySeparator(path[2]))
                {
                    num++;
                }
                return path.Substring(0, num);
            }
            return Directory.GetCurrentDirectory().Substring(0, 2);
        }
    }

    /// <summary>Creates a uniquely named, zero-byte temporary file on disk and returns the full path of that file.</summary>
    /// <returns>The full path of the temporary file.</returns>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs, such as no unique temporary file name is available.  
    /// -or-
    ///  This method was unable to create a temporary file.</exception>
    [FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
    public static string GetTempFileName()
    {
        FileStream fileStream = null;
        int num = 0;
        SecurityManager.EnsureElevatedPermissions();
        Random random = new Random();
        string tempPath = Path.GetTempPath();
        string text;
        do
        {
            int num2 = random.Next();
            text = Path.Combine(tempPath, "tmp" + (num2 + 1).ToString("x", CultureInfo.InvariantCulture) + ".tmp");
            try
            {
                fileStream = new FileStream(text, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read, 8192, false, (FileOptions)1);
            }
            catch (IOException ex)
            {
                if (ex._HResult != -2147024816 || num++ > 65536)
                {
                    throw;
                }
            }
            catch (UnauthorizedAccessException ex2)
            {
                if (num++ > 65536)
                {
                    throw new IOException(ex2.Message, ex2);
                }
            }
        }
        while (fileStream == null);
        fileStream.Close();
        return text;
    }

    /// <summary>Returns the path of the current user's temporary folder.</summary>
    /// <returns>The path to the temporary folder, ending with a backslash.</returns>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permissions.</exception>
    [EnvironmentPermission(SecurityAction.Demand, Unrestricted = true)]
    public static string GetTempPath()
    {
        SecurityManager.EnsureElevatedPermissions();
        string temp_path = Path.get_temp_path();
        if (temp_path.Length > 0 && temp_path[temp_path.Length - 1] != Path.DirectorySeparatorChar)
        {
            return temp_path + Path.DirectorySeparatorChar.ToString();
        }
        return temp_path;
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern string get_temp_path();

    /// <summary>Determines whether a path includes a file name extension.</summary>
    /// <param name="path">The path to search for an extension.</param>
    /// <returns>
    ///   <see langword="true" /> if the characters that follow the last directory separator (\\ or /) or volume separator (:) in the path include a period (.) followed by one or more characters; otherwise, <see langword="false" />.</returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path" /> contains one or more of the invalid characters defined in <see cref="M:System.IO.Path.GetInvalidPathChars" />.</exception>
    public static bool HasExtension(string path)
    {
        if (path == null || path.Trim().Length == 0)
        {
            return false;
        }
        if (path.IndexOfAny(Path.InvalidPathChars) != -1)
        {
            throw new ArgumentException("Illegal characters in path.");
        }
        int num = Path.findExtension(path);
        return 0 <= num && num < path.Length - 1;
    }

    /// <summary>Returns a random folder name or file name.</summary>
    /// <returns>A random folder name or file name.</returns>
    public static string GetRandomFileName()
    {
        StringBuilder stringBuilder = new StringBuilder(12);
        RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
        byte[] array = new byte[11];
        randomNumberGenerator.GetBytes(array);
        for (int i = 0; i < array.Length; i++)
        {
            if (stringBuilder.Length == 8)
            {
                stringBuilder.Append('.');
            }
            int num = (int)(array[i] % 36);
            char value = (char)((num < 26) ? (num + 97) : (num - 26 + 48));
            stringBuilder.Append(value);
        }
        return stringBuilder.ToString();
    }

    private static string GetServerAndShare(string path)
    {
        int num = 2;
        while (num < path.Length && !Path.IsDirectorySeparator(path[num]))
        {
            num++;
        }
        if (num < path.Length)
        {
            num++;
            while (num < path.Length && !Path.IsDirectorySeparator(path[num]))
            {
                num++;
            }
        }
        return path.Substring(2, num - 2).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
    }

    private static bool SameRoot(string root, string path)
    {
        if (root.Length < 2 || path.Length < 2)
        {
            return false;
        }
        if (!Path.IsDirectorySeparator(root[0]) || !Path.IsDirectorySeparator(root[1]))
        {
            return root[0].Equals(path[0]) && path[1] == Path.VolumeSeparatorChar && (root.Length <= 2 || path.Length <= 2 || (Path.IsDirectorySeparator(root[2]) && Path.IsDirectorySeparator(path[2])));
        }
        if (!Path.IsDirectorySeparator(path[0]) || !Path.IsDirectorySeparator(path[1]))
        {
            return false;
        }
        string serverAndShare = Path.GetServerAndShare(root);
        string serverAndShare2 = Path.GetServerAndShare(path);
        return string.Compare(serverAndShare, serverAndShare2, true, CultureInfo.InvariantCulture) == 0;
    }

    private static string CanonicalizePath(string path)
    {
        if (path == null)
        {
            return path;
        }
        if (Environment.IsRunningOnWindows)
        {
            path = path.Trim();
        }
        if (path.Length == 0)
        {
            return path;
        }
        string pathRoot = Path.GetPathRoot(path);
        string[] array = path.Split(new char[]
        {
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar
        });
        int num = 0;
        bool flag = Environment.IsRunningOnWindows && pathRoot.Length > 2 && Path.IsDirectorySeparator(pathRoot[0]) && Path.IsDirectorySeparator(pathRoot[1]);
        int num2 = flag ? 3 : 0;
        for (int i = 0; i < array.Length; i++)
        {
            if (Environment.IsRunningOnWindows)
            {
                array[i] = array[i].TrimEnd(Array.Empty<char>());
            }
            if (!(array[i] == ".") && (i == 0 || array[i].Length != 0))
            {
                if (array[i] == "..")
                {
                    if (num > num2)
                    {
                        num--;
                    }
                }
                else
                {
                    array[num++] = array[i];
                }
            }
        }
        if (num == 0 || (num == 1 && array[0] == ""))
        {
            return pathRoot;
        }
        string text = string.Join(Path.DirectorySeparatorStr, array, 0, num);
        if (!Environment.IsRunningOnWindows)
        {
            if (pathRoot != "" && text.Length > 0 && text[0] != '/')
            {
                text = pathRoot + text;
            }
            return text;
        }
        if (flag)
        {
            text = Path.DirectorySeparatorStr + text;
        }
        if (!Path.SameRoot(pathRoot, text))
        {
            text = pathRoot + text;
        }
        if (flag)
        {
            return text;
        }
        if (!Path.IsDirectorySeparator(path[0]) && Path.SameRoot(pathRoot, path))
        {
            if (text.Length <= 2 && !text.EndsWith(Path.DirectorySeparatorStr))
            {
                text += Path.DirectorySeparatorChar.ToString();
            }
            return text;
        }
        string currentDirectory = Directory.GetCurrentDirectory();
        if (currentDirectory.Length > 1 && currentDirectory[1] == Path.VolumeSeparatorChar)
        {
            if (text.Length == 0 || Path.IsDirectorySeparator(text[0]))
            {
                text += "\\";
            }
            return currentDirectory.Substring(0, 2) + text;
        }
        if (Path.IsDirectorySeparator(currentDirectory[currentDirectory.Length - 1]) && Path.IsDirectorySeparator(text[0]))
        {
            return currentDirectory + text.Substring(1);
        }
        return currentDirectory + text;
    }

    internal static bool IsPathSubsetOf(string subset, string path)
    {
        if (subset.Length > path.Length)
        {
            return false;
        }
        int num = subset.LastIndexOfAny(Path.PathSeparatorChars);
        if (string.Compare(subset, 0, path, 0, num) != 0)
        {
            return false;
        }
        num++;
        int num2 = path.IndexOfAny(Path.PathSeparatorChars, num);
        if (num2 >= num)
        {
            return string.Compare(subset, num, path, num, path.Length - num2) == 0;
        }
        return subset.Length == path.Length && string.Compare(subset, num, path, num, subset.Length - num) == 0;
    }

    /// <summary>Combines an array of strings into a path.</summary>
    /// <param name="paths">An array of parts of the path.</param>
    /// <returns>The combined paths.</returns>
    /// <exception cref="T:System.ArgumentException">One of the strings in the array contains one or more of the invalid characters defined in <see cref="M:System.IO.Path.GetInvalidPathChars" />.</exception>
    /// <exception cref="T:System.ArgumentNullException">One of the strings in the array is <see langword="null" />.</exception>
    public static string Combine(params string[] paths)
    {
        if (paths == null)
        {
            throw new ArgumentNullException("paths");
        }
        StringBuilder stringBuilder = new StringBuilder();
        int num = paths.Length;
        bool flag = false;
        foreach (string text in paths)
        {
            if (text == null)
            {
                throw new ArgumentNullException("One of the paths contains a null value", "paths");
            }
            if (text.Length != 0)
            {
                if (text.IndexOfAny(Path.InvalidPathChars) != -1)
                {
                    throw new ArgumentException("Illegal characters in path.");
                }
                if (flag)
                {
                    flag = false;
                    stringBuilder.Append(Path.DirectorySeparatorStr);
                }
                num--;
                if (Path.IsPathRooted(text))
                {
                    stringBuilder.Length = 0;
                }
                stringBuilder.Append(text);
                int length = text.Length;
                if (length > 0 && num > 0)
                {
                    char c = text[length - 1];
                    if (c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar && c != Path.VolumeSeparatorChar)
                    {
                        flag = true;
                    }
                }
            }
        }
        return stringBuilder.ToString();
    }

    /// <summary>Combines three strings into a path.</summary>
    /// <param name="path1">The first path to combine.</param>
    /// <param name="path2">The second path to combine.</param>
    /// <param name="path3">The third path to combine.</param>
    /// <returns>The combined paths.</returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path1" />, <paramref name="path2" />, or <paramref name="path3" /> contains one or more of the invalid characters defined in <see cref="M:System.IO.Path.GetInvalidPathChars" />.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path1" />, <paramref name="path2" />, or <paramref name="path3" /> is <see langword="null" />.</exception>
    public static string Combine(string path1, string path2, string path3)
    {
        if (path1 == null)
        {
            throw new ArgumentNullException("path1");
        }
        if (path2 == null)
        {
            throw new ArgumentNullException("path2");
        }
        if (path3 == null)
        {
            throw new ArgumentNullException("path3");
        }
        return Path.Combine(new string[]
        {
                path1,
                path2,
                path3
        });
    }

    /// <summary>Combines four strings into a path.</summary>
    /// <param name="path1">The first path to combine.</param>
    /// <param name="path2">The second path to combine.</param>
    /// <param name="path3">The third path to combine.</param>
    /// <param name="path4">The fourth path to combine.</param>
    /// <returns>The combined paths.</returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="path1" />, <paramref name="path2" />, <paramref name="path3" />, or <paramref name="path4" /> contains one or more of the invalid characters defined in <see cref="M:System.IO.Path.GetInvalidPathChars" />.</exception>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="path1" />, <paramref name="path2" />, <paramref name="path3" />, or <paramref name="path4" /> is <see langword="null" />.</exception>
    public static string Combine(string path1, string path2, string path3, string path4)
    {
        if (path1 == null)
        {
            throw new ArgumentNullException("path1");
        }
        if (path2 == null)
        {
            throw new ArgumentNullException("path2");
        }
        if (path3 == null)
        {
            throw new ArgumentNullException("path3");
        }
        if (path4 == null)
        {
            throw new ArgumentNullException("path4");
        }
        return Path.Combine(new string[]
        {
                path1,
                path2,
                path3,
                path4
        });
    }

    internal static void Validate(string path)
    {
        Path.Validate(path, "path");
    }

    internal static void Validate(string path, string parameterName)
    {
        if (path == null)
        {
            throw new ArgumentNullException(parameterName);
        }
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(Locale.GetText("Path is empty"));
        }
        if (path.IndexOfAny(Path.InvalidPathChars) != -1)
        {
            throw new ArgumentException(Locale.GetText("Path contains invalid chars"));
        }
        if (Environment.IsRunningOnWindows)
        {
            int num = path.IndexOf(':');
            if (num >= 0 && num != 1)
            {
                throw new ArgumentException(parameterName);
            }
        }
    }

    // (get) Token: 0x06002B63 RID: 11107 RVA: 0x00098AE0 File Offset: 0x00096CE0
    internal static string DirectorySeparatorCharAsString
    {
        get
        {
            return Path.DirectorySeparatorStr;
        }
    }

    // (get) Token: 0x06002B64 RID: 11108 RVA: 0x00098AE7 File Offset: 0x00096CE7
    internal static char[] TrimEndChars
    {
        get
        {
            if (!Environment.IsRunningOnWindows)
            {
                return Path.trimEndCharsUnix;
            }
            return Path.trimEndCharsWindows;
        }
    }

    internal static void CheckSearchPattern(string searchPattern)
    {
        int num;
        while ((num = searchPattern.IndexOf("..", StringComparison.Ordinal)) != -1)
        {
            if (num + 2 == searchPattern.Length)
            {
                throw new ArgumentException(Environment.GetResourceString("Search pattern cannot contain \"..\" to move up directories and can be contained only internally in file/directory names, as in \"a..b\"."));
            }
            if (searchPattern[num + 2] == Path.DirectorySeparatorChar || searchPattern[num + 2] == Path.AltDirectorySeparatorChar)
            {
                throw new ArgumentException(Environment.GetResourceString("Search pattern cannot contain \"..\" to move up directories and can be contained only internally in file/directory names, as in \"a..b\"."));
            }
            searchPattern = searchPattern.Substring(num + 2);
        }
    }

    internal static void CheckInvalidPathChars(string path, bool checkAdditional = false)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (PathInternal.HasIllegalCharacters(path, checkAdditional))
        {
            throw new ArgumentException(Environment.GetResourceString("Illegal characters in path."));
        }
    }

    internal static string InternalCombine(string path1, string path2)
    {
        if (path1 == null || path2 == null)
        {
            throw new ArgumentNullException((path1 == null) ? "path1" : "path2");
        }
        Path.CheckInvalidPathChars(path1, false);
        Path.CheckInvalidPathChars(path2, false);
        if (path2.Length == 0)
        {
            throw new ArgumentException(Environment.GetResourceString("Path cannot be the empty string or all whitespace."), "path2");
        }
        if (Path.IsPathRooted(path2))
        {
            throw new ArgumentException(Environment.GetResourceString("Second path fragment must not be a drive or UNC name."), "path2");
        }
        int length = path1.Length;
        if (length == 0)
        {
            return path2;
        }
        char c = path1[length - 1];
        if (c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar && c != Path.VolumeSeparatorChar)
        {
            return path1 + Path.DirectorySeparatorCharAsString + path2;
        }
        return path1 + path2;
    }

    [Obsolete("see GetInvalidPathChars and GetInvalidFileNameChars methods.")]
    public static readonly char[] InvalidPathChars;
    
    private static readonly bool dirEqualsVolume;
    
    internal const int MAX_PATH = 260;
    
    internal static readonly char[] trimEndCharsWindows = new char[]
    {
            '\t',
            '\n',
            '\v',
            '\f',
            '\r',
            ' ',
            '\u0085',
            '\u00a0'
    };

    internal static readonly char[] trimEndCharsUnix = new char[0];
    */
}
