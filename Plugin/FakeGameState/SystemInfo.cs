using System;
using HarmonyLib;

namespace UniTASPlugin.FakeGameState;

public static class SystemInfo
{
    public static void Init()
    {
        var deviceTypeEnum = AccessTools.TypeByName("UnityEngine.DeviceType");
        if (deviceTypeEnum != null)
        {
            var sysInfo = Traverse.Create(typeof(UnityEngine.SystemInfo));
            var deviceType = sysInfo.Property("deviceType");
            if (deviceType.PropertyExists())
            {
                // just in case, check
                var allVariants = Enum.GetValues(deviceTypeEnum);
                var foundDesktop = false;
                foreach (var variant in allVariants)
                {
                    if (variant.ToString() == DeviceType)
                        foundDesktop = true;
                }
                if (!foundDesktop)
                    Plugin.Instance.Log.LogError($"DeviceType enum doesn't contain {DeviceType}, fix this");
            }
        }
    }

    public static string DeviceType { get; set; } = "Desktop";
}
