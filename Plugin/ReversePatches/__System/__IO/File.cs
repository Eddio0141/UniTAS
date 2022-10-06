using HarmonyLib;
using System;
using FileOrig = System.IO.File;

namespace UniTASPlugin.ReversePatches.__System.__IO;

[HarmonyPatch]
public static class File
{
    public static bool Exists(string path)
    {
        return ExistsPatch.method(path);
    }

    public static string ReadAllText(string path)
    {
        return ReadAllTextPatch.method(path);
    }

    [HarmonyPatch]
    static class ExistsPatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.Exists))]
        public static bool method(string path)
        {
            throw new NotImplementedException();
        }
    }

    [HarmonyPatch]
    static class ReadAllTextPatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.ReadAllText), new Type[] { typeof(string) })]
        public static string method(string path)
        {
            throw new NotImplementedException();
        }
    }
}
