using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.FakeGameState.GameFileSystem;

namespace UniTASPlugin.Patches.System.IO;

[HarmonyPatch]
class GetDiskFreeSpace
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(DriveInfo), "GetDiskFreeSpace", new[] { typeof(string), typeof(ulong), typeof(ulong), typeof(ulong) });
    }

    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(string path, out ulong availableFreeSpace, out ulong totalSize, out ulong totalFreeSpace)
    {
        FileSystem.ExternalHelpers.GetDiskFreeSpace(path, out availableFreeSpace, out totalSize, out totalFreeSpace);
        return false;
    }
}