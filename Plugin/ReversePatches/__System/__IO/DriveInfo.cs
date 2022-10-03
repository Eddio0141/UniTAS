using HarmonyLib;
using System;
using System.Reflection;
using DriveInfoOrig = System.IO.DriveInfo;

namespace UniTASPlugin.ReversePatches.__System.__IO;

[HarmonyPatch]
public static class DriveInfo
{
    public static void GetDiskFreeSpace(string path, out ulong availableFreeSpace, out ulong totalSize, out ulong totalFreeSpace) =>
        GetDiskFreeSpacePatch.method(path, out availableFreeSpace, out totalSize, out totalFreeSpace);
    public static DriveInfoOrig[] GetDrives() => GetDrivesPatch.method();

    [HarmonyPatch]
    static class GetDiskFreeSpacePatch
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(DriveInfoOrig), "DriveInfoOrig.GetDiskFreeSpace")]
        public static void method(string path, out ulong availableFreeSpace, out ulong totalSize, out ulong totalFreeSpace)
        {
            throw new NotImplementedException();
        }
    }

    [HarmonyPatch]
    static class GetDrivesPatch
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(DriveInfoOrig), nameof(DriveInfoOrig.GetDrives))]
        public static DriveInfoOrig[] method()
        {
            throw new NotImplementedException();
        }
    }
}
