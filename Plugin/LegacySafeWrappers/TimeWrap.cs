using HarmonyLib;
using UniTASPlugin.ReverseInvoker;
using UnityEngine;

namespace UniTASPlugin.LegacySafeWrappers;

public static class TimeWrap
{
    public static readonly bool CaptureDeltaTimeExists = CaptureDeltaTimeTraverse.PropertyExists();
    private static Traverse CaptureDeltaTimeTraverse => Traverse.Create<Time>().Property("captureDeltaTime");

    public static float CaptureFrameTime
    {
        get => CaptureDeltaTimeExists ? CaptureDeltaTimeTraverse.GetValue<float>() : 1.0f / Time.captureFramerate;
        set
        {
            if (CaptureDeltaTimeExists)
            {
                var rev = Plugin.Kernel.GetInstance<IReverseInvokerFactory>().GetReverseInvoker();
                rev.Invoke(() => CaptureDeltaTimeTraverse.SetValue(value));
            }
            else
            {
                Time.captureFramerate = (int)(1 / value);
            }
        }
    }

    public static bool FrameTimeNotSet => CaptureDeltaTimeExists
        ? CaptureDeltaTimeTraverse.GetValue<float>() == 0
        : Time.captureFramerate == 0;
}