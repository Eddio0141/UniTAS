using System;
using System.Reflection;
using HarmonyLib;
using DriveInfoOrig = System.IO.DriveInfo;

namespace UniTASPlugin.ReversePatches.__System.__IO;

[HarmonyPatch]
public static class DriveInfo
{
    public static void GetDiskFreeSpace(string path, out ulong availableFreeSpace, out ulong totalSize, out ulong totalFreeSpace)
    {
        GetDiskFreeSpacePatch.method(path, out availableFreeSpace, out totalSize, out totalFreeSpace);
    }

    public static DriveInfoOrig[] GetDrives()
    {
        return GetDrivesPatch.method();
    }

    [HarmonyPatch]
    private static class GetDiskFreeSpacePatch
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
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
    private static class GetDrivesPatch
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
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
