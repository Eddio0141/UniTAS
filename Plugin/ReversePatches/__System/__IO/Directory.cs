using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using DirOrig = System.IO.Directory;

namespace UniTASPlugin.ReversePatches.__System.__IO;

[HarmonyPatch]
public static class Directory
{
    public static bool Exists(string path) => ExistsPatch.method(path);
    public static string[] GetFiles(string path) => GetFilesPatch.method(path);
    static string[] InternalGetFiles(string path, string searchPattern, SearchOption searchOption) => InternalGetFilesPatch.method(path, searchPattern, searchOption);
    static string[] InternalGetFileDirectoryNames
        (string path, string userPathOriginal, string searchPattern, bool includeFiles, bool includeDirs, SearchOption searchOption, bool checkHost) =>
        InternalGetFileDirectoryNamesPatch.method(path, userPathOriginal, searchPattern, includeFiles, includeDirs, searchOption, checkHost);

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
}
