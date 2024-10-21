using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services.Movie;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Overlays;

[Singleton]
[ExcludeRegisterIfTesting]
public class MovieEndStatus : BuiltInOverlay
{
    private float _messageDisplayLeft;

    public MovieEndStatus(WindowDependencies windowDependencies, IMovieRunnerEvents movieRunnerEvents) : base(
        windowDependencies, "Movie end status")
    {
        movieRunnerEvents.OnMovieRunningStatusChange += OnMovieRunningStatusChange;
    }

    private const float MessageDisplayTime = 1f;

    protected override AnchoredOffset DefaultOffset => new(1, 1, -210, -5);
    protected override int DefaultFontSize => 20;

    protected override string Update()
    {
        _messageDisplayLeft -= Time.deltaTime;
        return _messageDisplayLeft <= 0 ? "" : "Movie End";
    }

    private void OnMovieRunningStatusChange(bool running)
    {
        if (running) return;
        _messageDisplayLeft = MessageDisplayTime;
    }
}