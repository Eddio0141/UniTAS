using HarmonyLib;
using PathOrig = System.IO.Path;

namespace UniTASPlugin.ReversePatches.__System.__IO;

[HarmonyPatch]
public static class Path
{
    public static char DirectorySeparatorChar => PathOrig.DirectorySeparatorChar;
    public static string Combine(string path1, string path2)
    {
        return PathOrig.Combine(path1, path2);
    }

    public static string GetFileName(string path)
    {
        return PathOrig.GetFileName(path);
    }

    public static string GetDirectoryName(string path)
    {
        return PathOrig.GetDirectoryName(path);
    }

    public static string GetFileNameWithoutExtension(string path)
    {
        return PathOrig.GetFileNameWithoutExtension(path);
    }

    public static string GetFullPath(string path)
    {
        return PathOrig.GetFullPath(path);
    }
}
