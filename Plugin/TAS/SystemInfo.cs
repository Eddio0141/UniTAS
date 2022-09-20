using HarmonyLib;
using System;

namespace UniTASPlugin.TAS;

public static class SystemInfo
{
    public static void Init()
    {
        Type deviceTypeEnum = AccessTools.TypeByName("UnityEngine.DeviceType");
        if (deviceTypeEnum != null)
        {
            Traverse sysInfo = Traverse.Create(typeof(UnityEngine.SystemInfo));
            Traverse deviceType = sysInfo.Property("deviceType");
            if (deviceType.PropertyExists())
            {
                // just in case, check
                Array allVariants = Enum.GetValues(deviceTypeEnum);
                bool foundDesktop = false;
                foreach (object variant in allVariants)
                {
                    if (variant.ToString() == DeviceType)
                        foundDesktop = true;
                }
                if (!foundDesktop)
                    Plugin.Log.LogError($"DeviceType enum doesn't contain {DeviceType}, fix this");
            }
        }
    }

    public static string DeviceType { get; set; } = "Desktop";
}
