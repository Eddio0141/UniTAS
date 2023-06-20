using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.Movie;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.VirtualEnvironment;

namespace UniTAS.Patcher.Implementations.GUI.Overlays;

[ForceInstantiate]
[Singleton]
[ExcludeRegisterIfTesting]
public class TimeOverlay : BuiltInOverlay, IOnMovieRunningStatusChange
{
    private bool _update;
    private readonly ITimeEnv _timeEnv;

    public TimeOverlay(IConfig config, IDrawing drawing, ITimeEnv timeEnv) : base(config, drawing)
    {
        _timeEnv = timeEnv;
        Text = "0.000";
    }

    protected override AnchoredOffset DefaultOffset => new(0, 0, 0, 30);
    protected override string ConfigValue => "Time";

    protected override void Update()
    {
        if (!_update) return;

        var time = TimeSpan.FromSeconds(_timeEnv.SecondsSinceStartUp);

        var hour = time.Hours > 0;
        Text = "";
        if (hour)
        {
            Text += $"{time.Hours}:";

            if (time.Minutes < 10)
            {
                Text += "0";
            }
        }

        var minute = time.Minutes > 0 || hour;
        if (minute)
        {
            Text += $"{time.Minutes}:";

            if (time.Seconds < 10)
            {
                Text += "0";
            }
        }

        Text += $"{time.Seconds}.{time.Milliseconds:D3}";
    }

    public void OnMovieRunningStatusChange(bool running)
    {
        _update = running;
    }
}