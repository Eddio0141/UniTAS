using HarmonyLib;
using System;

namespace UniTASPlugin.Patches;

[HarmonyPatch(typeof(UnityEngine.SystemInfo), "deviceType", MethodType.Getter)]
class SystemInfo
{
    static Exception Cleanup(System.Reflection.MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref object __result)
    {
        Type deviceType = AccessTools.TypeByName("UnityEngine.DeviceType");
        __result = Enum.Parse(deviceType, TAS.SystemInfo.DeviceType);
        return false;
    }
}
