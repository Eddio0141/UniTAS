using HarmonyLib;
using System;

namespace UniTASPlugin.Patches.__UnityEngine;

[HarmonyPatch(typeof(UnityEngine.SystemInfo), "deviceType", MethodType.Getter)]
class SystemInfo
{
    static Exception Cleanup(System.Reflection.MethodBase original, Exception ex)
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
