using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using DirOrig = System.IO.Directory;

namespace UniTASPlugin.ReversePatches.__System.__IO;

[HarmonyPatch]
public static class Directory
{
    public static bool Exists(string path)
    {
        return ExistsPatch.method(path);
    }

    public static string[] GetFiles(string path)
    {
        return GetFilesPatch.method(path);
    }

    static string[] InternalGetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return InternalGetFilesPatch.method(path, searchPattern, searchOption);
    }

    static string[] InternalGetFileDirectoryNames
            (string path, string userPathOriginal, string searchPattern, bool includeFiles, bool includeDirs, SearchOption searchOption, bool checkHost)
    {
        return InternalGetFileDirectoryNamesPatch.method(path, userPathOriginal, searchPattern, includeFiles, includeDirs, searchOption, checkHost);
    }

    public static string[] GetFileSystemEntries(string path, string searchPattern)
    {
        return GetFileSystemEntriesPatch.method(path, searchPattern);
    }

    [HarmonyPatch]
    static class ExistsPatch
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.Exists))]
        public static bool method(string path)
        {
            throw new NotImplementedException();
        }
    }

    [HarmonyPatch]
    static class GetFilesPatch
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.GetFiles), new Type[] { typeof(string) })]
        public static string[] method(string path)
        {
            throw new NotImplementedException();
        }
    }

    [HarmonyPatch]
    static class InternalGetFilesPatch
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(DirOrig), "InternalGetFiles")]
        public static string[] method(string path, string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }
    }

    [HarmonyPatch]
    static class InternalGetFileDirectoryNamesPatch
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(DirOrig), "InternalGetFileDirectoryNames")]
        public static string[] method(string path, string userPathOriginal, string searchPattern, bool includeFiles, bool includeDirs, SearchOption searchOption, bool checkHost)
        {
            throw new NotImplementedException();
        }
    }

    [HarmonyPatch]
    static class GetFileSystemEntriesPatch
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.GetFileSystemEntries), new Type[] { typeof(string), typeof(string) })]
        public static string[] method(string path, string searchPattern)
        {
            throw new NotImplementedException();
        }
    }
}
