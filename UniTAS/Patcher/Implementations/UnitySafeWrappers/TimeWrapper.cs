using System;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
[ExcludeRegisterIfTesting]
public class TimeWrapper : ITimeWrapper
{
    private readonly Action<float> _captureDeltaTimeSet;
    private readonly Func<float> _captureDeltaTimeGet;

    private readonly IPatchReverseInvoker _reverseInvoker;
    private readonly ILogger _logger;

    public TimeWrapper(IPatchReverseInvoker reverseInvoker, ILogger logger)
    {
        var captureDt = AccessTools.Property(typeof(Time), "captureDeltaTime");
        if (captureDt != null)
        {
            _captureDeltaTimeSet = AccessTools.MethodDelegate<Action<float>>(captureDt.GetSetMethod());
            _captureDeltaTimeGet = AccessTools.MethodDelegate<Func<float>>(captureDt.GetGetMethod());
        }

        _reverseInvoker = reverseInvoker;
        _logger = logger;
    }

    public bool IntFPSOnly => _captureDeltaTimeSet == null;

    public double CaptureFrameTime
    {
        get
        {
            if (_captureDeltaTimeGet != null)
            {
                return _captureDeltaTimeGet.Invoke();
            }

            var fps = Time.captureFramerate;
            return fps > 0 ? 1.0 / fps : 0.0;
        }
        set
        {
            if (_captureDeltaTimeSet != null)
            {
                if (value <= 0.0)
                {
                    _logger.LogError(
                        "Setting game frame time to 0 or lower is invalid and will cause issues, using 0.001 instead");
                    value = 0.001;
                }

                _logger.LogDebug($"Setting captureDeltaTime to {value}");
                _reverseInvoker.Invoke(() => _captureDeltaTimeSet.Invoke((float)value));
            }
            else
            {
                // round to nearest int
                var fps = value > 0.0 ? (int)Math.Round(1.0 / value) : 0;
                if (fps <= 0)
                {
                    _logger.LogError(
                        "Setting game fps to 0 or lower is invalid and will cause issues, using 1 instead");
                    fps = 1;
                }

                _logger.LogDebug($"Setting captureFramerate to {fps}");
                _reverseInvoker.Invoke(setFps => Time.captureFramerate = setFps, fps);
            }
        }
    }
}