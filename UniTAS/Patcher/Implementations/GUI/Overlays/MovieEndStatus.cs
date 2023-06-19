using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.Movie;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Overlays;

[Singleton]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
public class MovieEndStatus : BuiltInOverlay, IOnMovieRunningStatusChange
{
    private float _messageDisplayLeft;
    private const float MESSAGE_DISPLAY_TIME = 1f;

    public MovieEndStatus(IConfig config, IOverlayDrawing overlayDrawing) : base(config, overlayDrawing)
    {
    }

    protected override AnchoredOffset DefaultOffset => new(1, 1, 0, 0);
    protected override string ConfigValue => "MovieEndStatus";
    protected override int DefaultFontSize => 20;

    protected override void Update()
    {
        _messageDisplayLeft -= Time.deltaTime;
        Text = _messageDisplayLeft <= 0 ? "" : "Movie End";
    }

    public void OnMovieRunningStatusChange(bool running)
    {
        if (running) return;
        _messageDisplayLeft = MESSAGE_DISPLAY_TIME;
    }
}