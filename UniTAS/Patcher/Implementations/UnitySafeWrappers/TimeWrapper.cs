using System;
using System.Reflection;
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
    private readonly PropertyInfo _captureDeltaTime = typeof(Time).GetProperty("captureDeltaTime");

    private readonly IPatchReverseInvoker _reverseInvoker;
    private readonly ILogger _logger;

    public TimeWrapper(IPatchReverseInvoker reverseInvoker, ILogger logger)
    {
        _reverseInvoker = reverseInvoker;
        _logger = logger;
    }

    public bool IntFPSOnly => _captureDeltaTime == null;

    public double CaptureFrameTime
    {
        get
        {
            if (_captureDeltaTime != null)
            {
                // var value = _reverseInvoker.Invoke(() => _captureDeltaTime.GetValue(null, null));
                return (float)_captureDeltaTime.GetValue(null, null);
            }

            // var value = _reverseInvoker.Invoke(() => Time.captureFramerate);
            var fps = Time.captureFramerate;
            return fps > 0 ? 1.0 / fps : 0.0;
        }
        set
        {
            if (_captureDeltaTime != null)
            {
                if (value <= 0.0)
                {
                    _logger.LogError(
                        "Setting game frame time to 0 or lower is invalid and will cause issues, using 0.001 instead");
                    value = 0.001;
                }

                _logger.LogDebug($"Setting captureDeltaTime to {value}");
                _reverseInvoker.Invoke(() => _captureDeltaTime.SetValue(null, (float)value, null));
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