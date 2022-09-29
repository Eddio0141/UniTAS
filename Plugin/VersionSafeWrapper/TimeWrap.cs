using HarmonyLib;
using UnityEngine;
using TimeOrig = UniTASPlugin.ReversePatches.__UnityEngine.Time;

namespace UniTASPlugin.VersionSafeWrapper;

public static class TimeWrap
{
    static Traverse captureDeltaTime()
    {
        return Traverse.Create<Time>().Property("captureDeltaTime");
    }

    public static bool HasCaptureDeltaTime()
    {
        return captureDeltaTime().PropertyExists();
    }

    public static float captureFrametime
    {
        get => HasCaptureDeltaTime() ? captureDeltaTime().GetValue<float>() : 1 / Time.captureFramerate;
        set
        {
            if (HasCaptureDeltaTime())
            {
                TimeOrig.captureDeltaTime.set(value);
            }
            else
            {
                TimeOrig.captureFramerate.set((int)(1 / value));
            }
        }
    }

    public static bool FrametimeNotSet => captureDeltaTime().PropertyExists() ? captureDeltaTime().GetValue<float>() == 0 : Time.captureFramerate == 0;
}