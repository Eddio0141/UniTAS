using HarmonyLib;
using UnityEngine;

namespace UniTASPlugin.VersionSafeWrapper;

public static class TimeWrap
{
    public static bool SettingFrametime { get; private set; } = false;

    static Traverse captureDeltaTime()
    {
        return Traverse.Create<Time>().Property("captureDeltaTime");
    }

    static Traverse captureFramerate()
    {
        return Traverse.Create<Time>().Property("captureFramerate");
    }

    public static bool HasCaptureDeltaTime()
    {
        return captureDeltaTime().PropertyExists();
    }

    public static float captureFrametime
    {
        get => captureDeltaTime().PropertyExists() ? captureDeltaTime().GetValue<float>() : 1 / captureFramerate().GetValue<int>();
        set
        {
            SettingFrametime = true;
            if (captureDeltaTime().PropertyExists())
            {
                _ = captureDeltaTime().SetValue(value);
            }
            else
            {
                var framerate = (int)(1 / value);
                _ = captureFramerate().SetValue(framerate);
            }
            SettingFrametime = false;
        }
    }

    public static bool FrametimeNotSet => captureDeltaTime().PropertyExists() ? captureDeltaTime().GetValue<float>() == 0 : captureFramerate().GetValue<int>() == 0;
}