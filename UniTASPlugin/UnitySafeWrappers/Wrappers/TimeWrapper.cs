using System;
using System.Diagnostics;
using System.Reflection;
using UniTASPlugin.ReverseInvoker;
using UniTASPlugin.UnitySafeWrappers.Interfaces;
using UnityEngine;

namespace UniTASPlugin.UnitySafeWrappers.Wrappers;

// ReSharper disable once ClassNeverInstantiated.Global
public class TimeWrapper : ITimeWrapper
{
    private readonly PropertyInfo _captureDeltaTime = typeof(Time).GetProperty("captureDeltaTime");

    private readonly IPatchReverseInvoker _reverseInvoker;

    public TimeWrapper(IPatchReverseInvoker reverseInvoker)
    {
        _reverseInvoker = reverseInvoker;
    }

    public float CaptureFrameTime
    {
        get
        {
            // TODO in the future this will be invoked with reverseInvoker
            if (_captureDeltaTime != null)
            {
                // var value = _reverseInvoker.Invoke(() => _captureDeltaTime.GetValue(null, null));
                return (float)_captureDeltaTime.GetValue(null, null);
            }

            // var value = _reverseInvoker.Invoke(() => Time.captureFramerate);
            var fps = Time.captureFramerate;
            return fps > 0 ? 1.0f / fps : 0f;
        }
        set
        {
            if (_captureDeltaTime != null)
            {
                Trace.Write($"Setting captureDeltaTime to {value}");
                _reverseInvoker.Invoke(() => _captureDeltaTime.SetValue(null, null, new object[] { value }));
            }
            else
            {
                var fps = value > 0f ? (int)Math.Round(1.0f / value) : 0;
                Trace.Write($"Setting captureFramerate to {fps}");
                _reverseInvoker.Invoke(() => Time.captureFramerate = fps);
            }
        }
    }

    public float FixedDeltaTime => Time.fixedDeltaTime;
    public float DeltaTime => Time.deltaTime;

    public int FrameCount => _reverseInvoker.Invoke(() => Time.frameCount);
}