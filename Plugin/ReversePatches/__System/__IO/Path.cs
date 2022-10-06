using HarmonyLib;
using System;
using System.Reflection;
using PathOrig = System.IO.Path;

namespace UniTASPlugin.ReversePatches.__System.__IO;

[HarmonyPatch]
public static class Path
{
    public static char DirectorySeparatorChar { get => PathOrig.DirectorySeparatorChar; }
    public static string Combine(string path1, string path2) => PathOrig.Combine(path1, path2);
    public static string GetFileName(string path) => PathOrig.GetFileName(path);
    public static string GetDirectoryName(string path) => PathOrig.GetDirectoryName(path);
    public static string GetFileNameWithoutExtension(string path) => PathOrig.GetFileNameWithoutExtension(path);
    public static string GetFullPath(string path) => PathOrig.GetFullPath(path);
}
