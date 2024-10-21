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

    public FrameCountOverlay(WindowDependencies windowDependencies, IMovieRunnerEvents movieRunnerEvents) : base(
        windowDependencies, "Frame count")
    {
        movieRunnerEvents.OnMovieRunningStatusChange += OnMovieRunningStatusChange;
    }

    protected override AnchoredOffset DefaultOffset => new(0, 0, 0, 30);

    private void OnMovieRunningStatusChange(bool running)
    {
        if (running)
        {
            _frameCount = 0;
            _update = true;
        }
        else
        {
            _update = false;
        }
    }

    protected override string Update()
    {
        if (_update)
        {
            _frameCount++;
        }

        return $"Frame: {_frameCount.ToString()}";
    }
}