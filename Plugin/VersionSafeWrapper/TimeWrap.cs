using HarmonyLib;
using UnityEngine;

namespace UniTASPlugin.VersionSafeWrapper;

public static class TimeWrap
{
    private static bool settingFrametime = false;

    public static bool SettingFrametime { get => settingFrametime; }

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
        get
        {
            if (captureDeltaTime().PropertyExists())
            {
                return captureDeltaTime().GetValue<float>();
            }
            return 1 / captureFramerate().GetValue<int>();
        }
        set
        {
            settingFrametime = true;
            if (captureDeltaTime().PropertyExists())
            {
                captureDeltaTime().SetValue(value);
            }
            else
            {
                int framerate = (int)(1 / value);
                captureFramerate().SetValue(framerate);
            }
            settingFrametime = false;
        }
    }

    public static bool FrametimeNotSet
    {
        get
        {
            if (captureDeltaTime().PropertyExists())
            {
                return captureDeltaTime().GetValue<float>() == 0;
            }
            return captureFramerate().GetValue<int>() == 0;
        }
    }
}