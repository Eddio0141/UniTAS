using System;
using HarmonyLib;
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
    private static class ExistsPatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.Exists))]
        public static bool method(string path)
        {
            throw new NotImplementedException();
        }
    }

    [HarmonyPatch]
    private static class ReadAllTextPatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(FileOrig), nameof(FileOrig.ReadAllText), typeof(string))]
        public static string method(string path)
        {
            throw new NotImplementedException();
        }
    }
}
