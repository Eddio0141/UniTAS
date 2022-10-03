using HarmonyLib;
using System;
using System.Reflection;
using PathOrig = System.IO.Path;

namespace UniTASPlugin.ReversePatches.__System.__IO;

[HarmonyPatch]
public static class Path
{
    public static char DirectorySeparatorChar { get => PathOrig.DirectorySeparatorChar; }
    public static string Combine(string path1, string path2) => CombinePatch.method(path1, path2);
    public static string GetFileName(string path) => GetFileNamePatch.method(path);
    public static string GetDirectoryName(string path) => GetDirectoryNamePatch.method(path);
    public static string GetFileNameWithoutExtension(string path) => GetFileNameWithoutExtensionPatch.method(path);

    [HarmonyPatch]
    static class CombinePatch
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(PathOrig), nameof(PathOrig.Combine), new Type[] { typeof(string), typeof(string) })]
        public static string method(string path1, string path2)
        {
            throw new NotImplementedException();
        }
    }

    [HarmonyPatch]
    static class GetFileNamePatch
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(PathOrig), nameof(PathOrig.GetFileName))]
        public static string method(string path)
        {
            throw new NotImplementedException();
        }
    }

    [HarmonyPatch]
    static class GetDirectoryNamePatch
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(PathOrig), nameof(PathOrig.GetDirectoryName))]
        public static string method(string path)
        {
            throw new NotImplementedException();
        }
    }

    [HarmonyPatch]
    static class GetFileNameWithoutExtensionPatch
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(PathOrig), nameof(PathOrig.GetFileNameWithoutExtension))]
        public static string method(string path)
        {
            throw new NotImplementedException();
        }
    }
}
