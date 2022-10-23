using System;
using HarmonyLib;

namespace UniTASPlugin.Patches.UnityEngine;

[HarmonyPatch(typeof(global::UnityEngine.SystemInfo), "deviceType", MethodType.Getter)]
class SystemInfo
{
    static Exception Cleanup(global::System.Reflection.MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref object __result)
    {
        var deviceType = AccessTools.TypeByName("UnityEngine.DeviceType");
        __result = Enum.Parse(deviceType, FakeGameState.SystemInfo.DeviceType);
        return false;
    }
}
