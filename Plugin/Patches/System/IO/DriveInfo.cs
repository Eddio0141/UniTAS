//using System;
//using System.Reflection;
//using System.Runtime.InteropServices;
//using HarmonyLib;
//using Ninject;
//using UniTASPlugin.FakeGameState.GameFileSystem;
//using DriveInfoOrig = System.IO.DriveInfo;
// ReSharper disable UnusedMember.Local

//namespace UniTASPlugin.Patches.System.IO;

/*
[HarmonyPatch]
internal static class DriveInfo
{
    [HarmonyPatch]
    private class GetDiskFreeSpace
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(DriveInfoOrig), "GetDiskFreeSpace", new[] { typeof(string), typeof(ulong), typeof(ulong), typeof(ulong) });
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(string path, [Out] ulong availableFreeSpace, out ulong totalSize, out ulong totalFreeSpace)
        {
            if (Plugin.Instance.Kernel.Get<PatchReverseInvoker>().Invoking)
                return true;
            FileSystem.ExternalHelpers.GetDiskFreeSpace(path, out availableFreeSpace, out totalSize, out totalFreeSpace);
            return false;
        }
    }
}
*/