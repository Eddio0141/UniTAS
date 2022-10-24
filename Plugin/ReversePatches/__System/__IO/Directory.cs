﻿using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
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

    private static string[] InternalGetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return InternalGetFilesPatch.method(path, searchPattern, searchOption);
    }

    private static string[] InternalGetFileDirectoryNames
            (string path, string userPathOriginal, string searchPattern, bool includeFiles, bool includeDirs, SearchOption searchOption, bool checkHost)
    {
        return InternalGetFileDirectoryNamesPatch.method(path, userPathOriginal, searchPattern, includeFiles, includeDirs, searchOption, checkHost);
    }

    public static string[] GetFileSystemEntries(string path, string searchPattern)
    {
        return GetFileSystemEntriesPatch.method(path, searchPattern);
    }

    [HarmonyPatch]
    private static class ExistsPatch
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
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
    private static class GetFilesPatch
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.GetFiles), typeof(string))]
        public static string[] method(string path)
        {
            throw new NotImplementedException();
        }
    }

    [HarmonyPatch]
    private static class InternalGetFilesPatch
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
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
    private static class InternalGetFileDirectoryNamesPatch
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
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
    private static class GetFileSystemEntriesPatch
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(DirOrig), nameof(DirOrig.GetFileSystemEntries), typeof(string), typeof(string))]
        public static string[] method(string path, string searchPattern)
        {
            throw new NotImplementedException();
        }
    }
}
