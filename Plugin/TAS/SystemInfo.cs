using HarmonyLib;
using System;

namespace UniTASPlugin.TAS;

public static class SystemInfo
{
    static SystemInfo()
    {
        DeviceType = null;
        var deviceTypeEnum = AccessTools.TypeByName("UnityEngine.DeviceType");
        if (deviceTypeEnum != null)
        {
            var sysInfo = Traverse.Create(typeof(UnityEngine.SystemInfo));
            var deviceType = sysInfo.Property("deviceType");
            if (deviceType.PropertyExists())
            {
                // by default we go pc
                DeviceType = "Desktop";

                // just in case, check
                var allVariants = Enum.GetValues(deviceTypeEnum);
                var foundDesktop = false;
                foreach (var variant in allVariants)
                {
                    if (variant.ToString() == "Desktop")
                        foundDesktop = true;
                }
                if (!foundDesktop)
                    Plugin.Log.LogError("DeviceType enum doesn't contain Desktop, fix this");
            }
        }
    }

    public static string DeviceType { get; set; }
}
