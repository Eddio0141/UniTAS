using HarmonyLib;
using System;
using System.Reflection;
using UniTASPlugin.FakeGameState.GameFileSystem;
using DriveInfoOrig = System.IO.DriveInfo;

namespace UniTASPlugin.Patches.__System.__IO;

[HarmonyPatch(typeof(DriveInfoOrig), "GetDiskFreeSpace")]
class GetDiskFreeSpace
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(string path, ref ulong availableFreeSpace, ref ulong totalSize, ref ulong totalFreeSpace)
    {
        FileSystem.GetDiskFreeSpace(path, out availableFreeSpace, out totalSize, out totalFreeSpace);
        return false;
    }
}