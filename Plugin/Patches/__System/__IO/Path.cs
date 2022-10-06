using HarmonyLib;
using System;
using System.Reflection;
using UniTASPlugin.FakeGameState.GameFileSystem;
using PathOrig = System.IO.Path;
using DirOrig = System.IO.Directory;
using System.Text;
using System.Globalization;

namespace UniTASPlugin.Patches.__System.__IO;

[HarmonyPatch]
static class Path
{
    static class Helper
    {
        static readonly Traverse PathGetFullPathNameTraverse = Traverse.Create(typeof(PathOrig)).Method("GetFullPathName");
        static readonly Traverse DirInsecureGetCurrentDirectoryTraverse = Traverse.Create(typeof(DirOrig)).Method("InsecureGetCurrentDirectory");
        static readonly Traverse PathIsDirectorySeparatorTraverse = Traverse.Create(typeof(PathOrig)).Method("IsDirectorySeparator");

        public static string PathGetFullPathName(string path)
        {
            return PathGetFullPathNameTraverse.GetValue<string>(path);
        }

        public static string DirInsecureGetCurrentDirectory()
        {
            return DirInsecureGetCurrentDirectoryTraverse.GetValue<string>();
        }

        public static bool PathIsDirectorySeparator(char c)
        {
            return PathIsDirectorySeparatorTraverse.GetValue<bool>(c);
        }

        public static bool CallOriginal()
        {
            var trace = new System.Diagnostics.StackTrace();
            var traceFrames = trace.GetFrames();
            foreach (var frame in traceFrames)
            {
                var typeName = frame.GetMethod().DeclaringType.FullName;
                if (typeName.StartsWith("UniTASPlugin.ReversePatches"))
                {
                    return true;
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(PathOrig), "findExtension")]
    class findExtension
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref int __result, string path)
        {
            if (Helper.CallOriginal())
                return true;
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
            if (Helper.CallOriginal())
                return true;
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
            if (Helper.CallOriginal())
                return true;
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

    [HarmonyPatch(typeof(PathOrig), "CleanPath")]
    class CleanPath
    {
        static Exception Cleanup(MethodBase original, System.Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Postfix(ref string __result, string s)
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
            if (length == 1 && (c == FileSystem.ExternalHelpers.DirectorySeparatorChar || c == FileSystem.ExternalHelpers.AltDirectorySeparatorChar))
            {
                __result = s;
                return false;
            }
            for (int i = num3; i < length; i++)
            {
                char c2 = s[i];
                if (c2 == FileSystem.ExternalHelpers.DirectorySeparatorChar || c2 == FileSystem.ExternalHelpers.AltDirectorySeparatorChar)
                {
                    if (FileSystem.ExternalHelpers.DirectorySeparatorChar != FileSystem.ExternalHelpers.AltDirectorySeparatorChar && c2 == FileSystem.ExternalHelpers.AltDirectorySeparatorChar)
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
                        if (c2 == FileSystem.ExternalHelpers.DirectorySeparatorChar || c2 == FileSystem.ExternalHelpers.AltDirectorySeparatorChar)
                        {
                            num++;
                        }
                    }
                }
            }
            if (num == 0 && num2 == 0)
            {
                __result = s;
                return false;
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
                if (c3 != FileSystem.ExternalHelpers.DirectorySeparatorChar && c3 != FileSystem.ExternalHelpers.AltDirectorySeparatorChar)
                {
                    array[num4++] = c3;
                }
                else if (num4 + 1 != array.Length)
                {
                    array[num4++] = FileSystem.ExternalHelpers.DirectorySeparatorChar;
                    while (j < length - 1)
                    {
                        c3 = s[j + 1];
                        if (c3 != FileSystem.ExternalHelpers.DirectorySeparatorChar && c3 != FileSystem.ExternalHelpers.AltDirectorySeparatorChar)
                        {
                            break;
                        }
                        j++;
                    }
                }
                j++;
            }
            __result = new string(array);
            return false;
        }
    }

    [HarmonyPatch(typeof(PathOrig), nameof(PathOrig.GetDirectoryName))]
    class GetDirectoryName
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref string __result, string path)
        {
            if (Helper.CallOriginal())
                return true;
            if (path == string.Empty)
            {
                throw new ArgumentException("Invalid path");
            }
            if (path == null || PathOrig.GetPathRoot(path) == path)
            {
                __result = null;
                return false;
            }
            if (path.Trim().Length == 0)
            {
                throw new ArgumentException("Argument string consists of whitespace characters only.");
            }
            if (path.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) > -1)
            {
                throw new ArgumentException("Path contains invalid characters");
            }
            int num = path.LastIndexOfAny(FileSystem.ExternalHelpers.PathSeparatorChars);
            if (num == 0)
            {
                num++;
            }
            if (num <= 0)
            {
                __result = string.Empty;
                return false;
            }
            string text = path.Substring(0, num);
            int length = text.Length;
            if (length >= 2 && FileSystem.ExternalHelpers.DirectorySeparatorChar == '\\' && text[length - 1] == FileSystem.ExternalHelpers.VolumeSeparatorChar)
            {
                __result = text + FileSystem.ExternalHelpers.DirectorySeparatorChar.ToString();
                return false;
            }
            if (length == 1 && FileSystem.ExternalHelpers.DirectorySeparatorChar == '\\' && path.Length >= 2 && path[num] == FileSystem.ExternalHelpers.VolumeSeparatorChar)
            {
                __result = text + FileSystem.ExternalHelpers.VolumeSeparatorChar.ToString();
                return false;
            }
            __result = Traverse.Create(typeof(PathOrig)).Method("CleanPath").GetValue<string>(text);
            return false;
        }
    }

    [HarmonyPatch(typeof(PathOrig), nameof(PathOrig.GetExtension))]
    class GetExtension
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref string __result, string path)
        {
            if (Helper.CallOriginal())
                return true;
            if (path == null)
            {
                __result = null;
                return false;
            }
            if (path.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
            {
                throw new ArgumentException("Illegal characters in path.");
            }
            int num = Traverse.Create(typeof(PathOrig)).Method("findExtension").GetValue<int>(path);
            if (num > -1 && num < path.Length - 1)
            {
                __result = path.Substring(num);
                return false;
            }
            __result = string.Empty;
            return false;
        }
    }

    [HarmonyPatch(typeof(PathOrig), "WindowsDriveAdjustment")]
    class WindowsDriveAdjustment
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref string __result, ref string path)
        {
            if (Helper.CallOriginal())
                return true;
            if (path.Length < 2)
            {
                if (path.Length == 1 && (path[0] == '\\' || path[0] == '/'))
                {
                    __result = PathOrig.GetPathRoot(DirOrig.GetCurrentDirectory());
                    return false;
                }
                __result = path;
                return false;
            }
            else
            {
                if (path[1] != ':' || !char.IsLetter(path[0]))
                {
                    __result = path;
                    return false;
                }
                string text = Helper.DirInsecureGetCurrentDirectory();
                if (path.Length == 2)
                {
                    if (text[0] == path[0])
                    {
                        path = text;
                    }
                    else
                    {
                        path = Helper.PathGetFullPathName(path);
                    }
                }
                else if (path[2] != FileSystem.ExternalHelpers.DirectorySeparatorChar && path[2] != FileSystem.ExternalHelpers.AltDirectorySeparatorChar)
                {
                    if (text[0] == path[0])
                    {
                        path = PathOrig.Combine(text, path.Substring(2, path.Length - 2));
                    }
                    else
                    {
                        path = Helper.PathGetFullPathName(path);
                    }
                }
                __result = path;
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(PathOrig), "IsDirectorySeparator")]
    class IsDirectorySeparator
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref bool __result, char c)
        {
            if (Helper.CallOriginal())
                return true;
            __result = c == FileSystem.ExternalHelpers.DirectorySeparatorChar || c == FileSystem.ExternalHelpers.AltDirectorySeparatorChar;
            return false;
        }
    }

    [HarmonyPatch(typeof(PathOrig), nameof(PathOrig.GetPathRoot))]
    class GetPathRoot
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref string __result, string path)
        {
            if (Helper.CallOriginal())
                return true;
            if (path == null)
            {
                __result = null;
                return false;
            }
            if (path.Trim().Length == 0)
            {
                throw new ArgumentException("The specified path is not of a legal form.");
            }
            if (!PathOrig.IsPathRooted(path))
            {
                __result = string.Empty;
                return false;
            }
            if (FileSystem.ExternalHelpers.DirectorySeparatorChar == '/')
            {
                if (!Helper.PathIsDirectorySeparator(path[0]))
                {
                    __result = string.Empty;
                    return false;
                }
                __result = FileSystem.ExternalHelpers.DirectorySeparatorStr;
                return false;
            }
            else
            {
                int num = 2;
                if (path.Length == 1 && Helper.PathIsDirectorySeparator(path[0]))
                {
                    __result = FileSystem.ExternalHelpers.DirectorySeparatorStr;
                    return false;
                }
                if (path.Length < 2)
                {
                    __result = string.Empty;
                    return false;
                }
                if (Helper.PathIsDirectorySeparator(path[0]) && Helper.PathIsDirectorySeparator(path[1]))
                {
                    while (num < path.Length && !Helper.PathIsDirectorySeparator(path[num]))
                    {
                        num++;
                    }
                    if (num < path.Length)
                    {
                        num++;
                        while (num < path.Length && !Helper.PathIsDirectorySeparator(path[num]))
                        {
                            num++;
                        }
                    }
                    __result = FileSystem.ExternalHelpers.DirectorySeparatorStr + FileSystem.ExternalHelpers.DirectorySeparatorStr + path.Substring(2, num - 2).Replace(FileSystem.ExternalHelpers.AltDirectorySeparatorChar, FileSystem.ExternalHelpers.DirectorySeparatorChar);
                    return false;
                }
                if (Helper.PathIsDirectorySeparator(path[0]))
                {
                    __result = FileSystem.ExternalHelpers.DirectorySeparatorStr;
                    return false;
                }
                if (path[1] == FileSystem.ExternalHelpers.VolumeSeparatorChar)
                {
                    if (path.Length >= 3 && Helper.PathIsDirectorySeparator(path[2]))
                    {
                        num++;
                    }
                    __result = path.Substring(0, num);
                    return false;
                }
                __result = DirOrig.GetCurrentDirectory().Substring(0, 2);
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(PathOrig), nameof(PathOrig.GetTempPath))]
    class GetTempPath
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref string __result)
        {
            if (Helper.CallOriginal())
                return true;
            string temp_path = Traverse.Create(typeof(PathOrig)).Method("get_temp_path").GetValue<string>();
            if (temp_path.Length > 0 && temp_path[temp_path.Length - 1] != FileSystem.ExternalHelpers.DirectorySeparatorChar)
            {
                __result = temp_path + FileSystem.ExternalHelpers.DirectorySeparatorChar.ToString();
                return false;
            }
            __result = temp_path;
            return false;
        }
    }

    [HarmonyPatch(typeof(PathOrig), nameof(PathOrig.HasExtension))]
    class HasExtension
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref bool __result, string path)
        {
            if (Helper.CallOriginal())
                return true;
            if (path == null || path.Trim().Length == 0)
            {
                __result = false;
                return false;
            }
            if (path.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
            {
                throw new ArgumentException("Illegal characters in path.");
            }
            int num = Traverse.Create(typeof(PathOrig)).Method("findExtension").GetValue<int>(path);
            __result = 0 <= num && num < path.Length - 1;
            return false;
        }
    }

    [HarmonyPatch(typeof(PathOrig), "GetServerAndShare")]
    class GetServerAndShare
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref string __result, string path)
        {
            if (Helper.CallOriginal())
                return true;
            int num = 2;
            while (num < path.Length && !Helper.PathIsDirectorySeparator(path[num]))
            {
                num++;
            }
            if (num < path.Length)
            {
                num++;
                while (num < path.Length && !Helper.PathIsDirectorySeparator(path[num]))
                {
                    num++;
                }
            }
            __result = path.Substring(2, num - 2).Replace(FileSystem.ExternalHelpers.AltDirectorySeparatorChar, FileSystem.ExternalHelpers.DirectorySeparatorChar);
            return false;
        }
    }

    [HarmonyPatch(typeof(PathOrig), "SameRoot")]
    class SameRoot
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref bool __result, string root, string path)
        {
            if (Helper.CallOriginal())
                return true;
            if (root.Length < 2 || path.Length < 2)
            {
                __result = false;
                return false;
            }
            if (!Helper.PathIsDirectorySeparator(root[0]) || !Helper.PathIsDirectorySeparator(root[1]))
            {
                __result = root[0].Equals(path[0]) && path[1] == FileSystem.ExternalHelpers.VolumeSeparatorChar && (root.Length <= 2 || path.Length <= 2 || (Helper.PathIsDirectorySeparator(root[2]) && Helper.PathIsDirectorySeparator(path[2])));
                return false;
            }
            if (!Helper.PathIsDirectorySeparator(path[0]) || !Helper.PathIsDirectorySeparator(path[1]))
            {
                __result = false;
                return false;
            }
            var getServerAndShare = Traverse.Create(typeof(PathOrig)).Method("GetServerAndShare");
            string serverAndShare = getServerAndShare.GetValue<string>(root);
            string serverAndShare2 = getServerAndShare.GetValue<string>(path);
            __result = string.Compare(serverAndShare, serverAndShare2, true, CultureInfo.InvariantCulture) == 0;
            return false;
        }
    }

    [HarmonyPatch(typeof(PathOrig), "IsPathSubsetOf")]
    class IsPathSubsetOf
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref bool __result, string subset, string path)
        {
            if (Helper.CallOriginal())
                return true;
            if (subset.Length > path.Length)
            {
                __result = false;
                return false;
            }
            int num = subset.LastIndexOfAny(FileSystem.ExternalHelpers.PathSeparatorChars);
            if (string.Compare(subset, 0, path, 0, num) != 0)
            {
                __result = false;
                return false;
            }
            num++;
            int num2 = path.IndexOfAny(FileSystem.ExternalHelpers.PathSeparatorChars, num);
            if (num2 >= num)
            {
                __result = string.Compare(subset, num, path, num, path.Length - num2) == 0;
                return false;
            }
            __result = subset.Length == path.Length && string.Compare(subset, num, path, num, subset.Length - num) == 0;
            return false;
        }
    }

    [HarmonyPatch(typeof(PathOrig), nameof(PathOrig.Combine), new Type[] { typeof(string[]) })]
    class Combine__stringArray
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref string __result, string[] paths)
        {
            if (Helper.CallOriginal())
                return true;
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
                    if (text.IndexOfAny(FileSystem.ExternalHelpers.InvalidPathChars) != -1)
                    {
                        throw new ArgumentException("Illegal characters in path.");
                    }
                    if (flag)
                    {
                        flag = false;
                        stringBuilder.Append(FileSystem.ExternalHelpers.DirectorySeparatorStr);
                    }
                    num--;
                    if (PathOrig.IsPathRooted(text))
                    {
                        stringBuilder.Length = 0;
                    }
                    stringBuilder.Append(text);
                    int length = text.Length;
                    if (length > 0 && num > 0)
                    {
                        char c = text[length - 1];
                        if (c != FileSystem.ExternalHelpers.DirectorySeparatorChar && c != FileSystem.ExternalHelpers.AltDirectorySeparatorChar && c != FileSystem.ExternalHelpers.VolumeSeparatorChar)
                        {
                            flag = true;
                        }
                    }
                }
            }
            __result = stringBuilder.ToString();
            return false;
        }
    }

    [HarmonyPatch(typeof(PathOrig), "DirectorySeparatorCharAsString", MethodType.Getter)]
    class DirectorySeparatorCharAsString
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref string __result)
        {
            if (Helper.CallOriginal())
                return true;
            __result = FileSystem.ExternalHelpers.DirectorySeparatorStr;
            return false;
        }
    }

    [HarmonyPatch(typeof(PathOrig), "InternalCombine")]
    class InternalCombine
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref string __result, string path1, string path2)
        {
            if (Helper.CallOriginal())
                return true;
            if (path1 == null || path2 == null)
            {
                throw new ArgumentNullException((path1 == null) ? "path1" : "path2");
            }
            var checkInvalidPathChars = Traverse.Create(typeof(PathOrig)).Method("CheckInvalidPathChars");
            checkInvalidPathChars.GetValue(path1, false);
            checkInvalidPathChars.GetValue(path2, false);
            if (path2.Length == 0)
            {
                throw new ArgumentException("Path cannot be the empty string or all whitespace.", "path2");
            }
            if (PathOrig.IsPathRooted(path2))
            {
                throw new ArgumentException("Second path fragment must not be a drive or UNC name.", "path2");
            }
            int length = path1.Length;
            if (length == 0)
            {
                __result = path2;
                return false;
            }
            char c = path1[length - 1];
            if (c != FileSystem.ExternalHelpers.DirectorySeparatorChar && c != FileSystem.ExternalHelpers.AltDirectorySeparatorChar && c != FileSystem.ExternalHelpers.VolumeSeparatorChar)
            {
                __result = path1 + FileSystem.ExternalHelpers.DirectorySeparatorChar.ToString() + path2;
                return false;
            }
            __result = path1 + path2;
            return false;
        }
    }
}