using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.Movie;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Overlays;

[Singleton]
[ExcludeRegisterIfTesting]
public class MovieEndStatus : BuiltInOverlay, IOnMovieRunningStatusChange
{
    private float _messageDisplayLeft;
    private const float MESSAGE_DISPLAY_TIME = 1f;

    public MovieEndStatus(IConfig config, IDrawing drawing) : base(config, drawing)
    {
    }

    protected override AnchoredOffset DefaultOffset => new(1, 1, 0, 0);
    protected override string ConfigName => "MovieEndStatus";
    protected override int DefaultFontSize => 20;

    protected override string Update()
    {
        _messageDisplayLeft -= Time.deltaTime;
        return _messageDisplayLeft <= 0 ? "" : "Movie End";
    }

    public void OnMovieRunningStatusChange(bool running)
    {
        if (running) return;
        _messageDisplayLeft = MESSAGE_DISPLAY_TIME;
    }
}