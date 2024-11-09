using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services.Movie;
using UniTAS.Patcher.Services.VirtualEnvironment;

namespace UniTAS.Patcher.Implementations.GUI.Overlays;

[Singleton]
[ExcludeRegisterIfTesting]
public class TimeOverlay : BuiltInOverlay
{
    private bool _update;
    private string _text = "0.000";
    private readonly ITimeEnv _timeEnv;

    public TimeOverlay(WindowDependencies windowDependencies, ITimeEnv timeEnv, IMovieRunner movieRunner) :
        base(windowDependencies, "Time")
    {
        _timeEnv = timeEnv;
        movieRunner.OnMovieStart += () => { _update = true; };
        movieRunner.OnMovieEnd += () => { _update = false; };
    }

    protected override AnchoredOffset DefaultOffset => new(0, 0, 0, 60);

    protected override string Update()
    {
        if (!_update) return _text;

        var time = TimeSpan.FromSeconds(_timeEnv.SecondsSinceStartUp);

        var hour = time.Hours > 0;
        _text = "";
        if (hour)
        {
            _text += $"{time.Hours}:";

            if (time.Minutes < 10)
            {
                _text += "0";
            }
        }

        var minute = time.Minutes > 0 || hour;
        if (minute)
        {
            _text += $"{time.Minutes}:";

            if (time.Seconds < 10)
            {
                _text += "0";
            }
        }

        _text += $"{time.Seconds}.{time.Milliseconds:D3}";

        return _text;
    }
}