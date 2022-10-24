using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.FakeGameState.GameFileSystem;
// ReSharper disable UnusedMember.Local

namespace UniTASPlugin.Patches.System.IO;

[HarmonyPatch]
internal class GetDiskFreeSpace
{
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(DriveInfo), "GetDiskFreeSpace", new[] { typeof(string), typeof(ulong), typeof(ulong), typeof(ulong) });
    }

    private static Exception Cleanup(MethodBase original, Exception ex)
    {
        return PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    private static bool Prefix(string path, out ulong availableFreeSpace, out ulong totalSize, out ulong totalFreeSpace)
    {
        FileSystem.ExternalHelpers.GetDiskFreeSpace(path, out availableFreeSpace, out totalSize, out totalFreeSpace);
        return false;
    }
}