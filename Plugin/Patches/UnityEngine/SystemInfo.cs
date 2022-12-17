using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.ReverseInvoker;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment

namespace UniTASPlugin.Patches.UnityEngine;

[HarmonyPatch]
internal class SystemInfo
{
    [HarmonyPatch(typeof(global::UnityEngine.SystemInfo), "deviceType", MethodType.Getter)]
    private class deviceType
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref object __result)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking)
                return true;
            var deviceType = AccessTools.TypeByName("UnityEngine.DeviceType");
            __result = Enum.Parse(deviceType, FakeGameState.SystemInfo.DeviceType);
            return false;
        }
    }
}