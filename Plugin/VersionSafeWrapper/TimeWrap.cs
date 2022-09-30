using HarmonyLib;
using UniTASPlugin.ReversePatches.__UnityEngine;

namespace UniTASPlugin.VersionSafeWrapper;

public static class TimeWrap
{
    public static readonly bool CaptureDeltaTimeExists = Traverse.Create<UnityEngine.Time>().Property("captureDeltaTime").PropertyExists();

    public static float captureFrametime
    {
        get => CaptureDeltaTimeExists ? Time.captureDeltaTime : 1 / Time.captureFramerate;
        set
        {
            if (CaptureDeltaTimeExists)
            {
                Time.captureDeltaTime = value;
            }
            else
            {
                Time.captureFramerate = (int)(1 / value);
            }
        }
    }

    public static bool FrametimeNotSet => CaptureDeltaTimeExists ? Time.captureDeltaTime == 0 : Time.captureFramerate == 0;
}