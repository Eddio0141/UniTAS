using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services.Movie;

namespace UniTAS.Patcher.Implementations.GUI.Overlays;

[Singleton]
[ExcludeRegisterIfTesting]
public class FrameCountOverlay : BuiltInOverlay
{
    private uint _frameCount;

    private bool _update;

    public FrameCountOverlay(WindowDependencies windowDependencies, IMovieRunner movieRunner) : base(
        windowDependencies, "Frame count")
    {
        movieRunner.OnMovieStart += () =>
        {
            _frameCount = 0;
            _update = true;
        };
        movieRunner.OnMovieEnd += () => { _update = false; };
    }

    protected override AnchoredOffset DefaultOffset => new(0, 0, 0, 30);

    protected override string Update()
    {
        if (_update)
        {
            _frameCount++;
        }

        return $"Frame: {_frameCount.ToString()}";
    }
}