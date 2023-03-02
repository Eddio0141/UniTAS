using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using UniTAS.Plugin.GameEnvironment;
using UniTAS.Plugin.Interfaces.Update;

namespace UniTAS.Plugin.MainThreadSpeedController;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class MainThreadSpeedControl : IMainThreadSpeedControl, IOnUpdate
{
    private readonly VirtualEnvironment _virtualEnvironment;

    public float SpeedMultiplier
    {
        get => _speedMultiplier;
        set
        {
            _speedMultiplier = value;

            if (value == 0f)
            {
                _stopwatch.Stop();
            }
            else if (!_stopwatch.IsRunning)
            {
                _stopwatch.Start();
            }

            _stopwatch.Reset();
        }
    }

    private readonly Stopwatch _stopwatch = new();
    private float _speedMultiplier;

    public MainThreadSpeedControl(VirtualEnvironment virtualEnvironment)
    {
        _virtualEnvironment = virtualEnvironment;
    }

    public void Update()
    {
        if (SpeedMultiplier == 0 || !_virtualEnvironment.RunVirtualEnvironment) return;

        var frametime = _virtualEnvironment.FrameTime;

        // if the actual time passed is less than the time that should have passed, wait
        var waitTime = frametime * SpeedMultiplier - _stopwatch.Elapsed.TotalSeconds;
        if (waitTime <= 0) return;

        Thread.Sleep((int)(waitTime * 1000f));

        _stopwatch.Reset();
    }
}