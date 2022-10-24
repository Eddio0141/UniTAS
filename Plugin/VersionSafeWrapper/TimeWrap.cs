using HarmonyLib;
using UnityEngine;

namespace UniTASPlugin.VersionSafeWrapper;

public static class TimeWrap
{
    public static readonly bool CaptureDeltaTimeExists = Traverse.Create<Time>().Property("captureDeltaTime").PropertyExists();

    public static float captureFrametime
    {
        get => CaptureDeltaTimeExists ? ReversePatches.__UnityEngine.Time.captureDeltaTime : 1 / ReversePatches.__UnityEngine.Time.captureFramerate;
        set
        {
            if (CaptureDeltaTimeExists)
            {
                ReversePatches.__UnityEngine.Time.captureDeltaTime = value;
            }
            else
            {
                ReversePatches.__UnityEngine.Time.captureFramerate = (int)(1 / value);
            }
        }
    }

    public static bool FrametimeNotSet => CaptureDeltaTimeExists ? ReversePatches.__UnityEngine.Time.captureDeltaTime == 0 : ReversePatches.__UnityEngine.Time.captureFramerate == 0;
}