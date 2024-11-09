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

    public MovieEndStatus(WindowDependencies windowDependencies, IMovieRunner movieRunner) : base(
        windowDependencies, "Movie end status")
    {
        movieRunner.OnMovieEnd += () => { _messageDisplayLeft = MessageDisplayTime; };
    }

    private const float MessageDisplayTime = 1f;

    protected override AnchoredOffset DefaultOffset => new(1, 1, -210, -5);
    protected override int DefaultFontSize => 20;

    protected override string Update()
    {
        _messageDisplayLeft -= Time.deltaTime;
        return _messageDisplayLeft <= 0 ? "" : "Movie End";
    }
}