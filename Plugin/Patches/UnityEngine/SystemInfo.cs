using System;
using System.Reflection;
using HarmonyLib;

namespace UniTASPlugin.Patches.UnityEngine;

[HarmonyPatch(typeof(global::UnityEngine.SystemInfo), "deviceType", MethodType.Getter)]
internal class SystemInfo
{
    private static Exception Cleanup(MethodBase original, Exception ex)
    {
        return PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    private static bool Prefix(ref object __result)
    {
        var deviceType = AccessTools.TypeByName("UnityEngine.DeviceType");
        __result = Enum.Parse(deviceType, FakeGameState.SystemInfo.DeviceType);
        return false;
    }
}
