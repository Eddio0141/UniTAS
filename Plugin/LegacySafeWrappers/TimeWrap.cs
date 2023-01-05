using System;
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
            var rev = Plugin.Kernel.GetInstance<IReverseInvokerFactory>().GetReverseInvoker();
            if (CaptureDeltaTimeExists)
            {
                rev.Invoke(() => CaptureDeltaTimeTraverse.SetValue(value));
            }
            else
            {
                rev.Invoke(() => Time.captureFramerate = (int)Math.Round(1.0f / value));
            }
        }
    }

    public static bool FrameTimeNotSet
    {
        get
        {
            var rev = Plugin.Kernel.GetInstance<IReverseInvokerFactory>().GetReverseInvoker();
            return CaptureDeltaTimeExists
                ? rev.Invoke(() => CaptureDeltaTimeTraverse.GetValue<float>() == 0)
                : rev.Invoke(() => Time.captureFramerate == 0);
        }
    }
}