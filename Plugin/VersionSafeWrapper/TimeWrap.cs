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

    public static void SetFrametime(float frametime)
    {
        settingFrametime = true;
        if (captureDeltaTime().PropertyExists())
        {
            captureDeltaTime().SetValue(frametime);
        }
        else
        {
            int framerate = (int)(1 / frametime);
            captureFramerate().SetValue(framerate);
        }
        settingFrametime = false;
    }
}