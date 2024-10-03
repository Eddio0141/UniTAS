using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.Movie;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.VirtualEnvironment;

namespace UniTAS.Patcher.Implementations.GUI.Overlays;

[Singleton]
[ExcludeRegisterIfTesting]
public class TimeOverlay : BuiltInOverlay, IOnMovieRunningStatusChange
{
    private bool _update;
    private readonly ITimeEnv _timeEnv;
    private string _text;

    public TimeOverlay(IConfig config, IDrawing drawing, ITimeEnv timeEnv) : base(config, drawing)
    {
        _timeEnv = timeEnv;
        _text = "0.000";
    }

    protected override AnchoredOffset DefaultOffset => new(0, 0, 0, 30);
    protected override string ConfigName => "Time";

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

    public void OnMovieRunningStatusChange(bool running)
    {
        _update = running;
    }
}