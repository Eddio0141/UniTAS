using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services.Movie;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Overlays;

[Singleton]
[ExcludeRegisterIfTesting]
public class FixedFrameCountOverlay : BuiltInOverlay
{
    private uint _fixedFrameCount;

    private bool _update;

    public FixedFrameCountOverlay(WindowDependencies windowDependencies, IMovieRunnerEvents movieRunnerEvents) : base(
        windowDependencies, "Fixed frames count", showByDefault: false)
    {
        windowDependencies.UpdateEvents.OnFixedUpdateActual += FixedUpdateActual;
        movieRunnerEvents.OnMovieRunningStatusChange += OnMovieRunningStatusChange;
    }

    protected override AnchoredOffset DefaultOffset => new(0, 0, 200, 30);

    private void OnMovieRunningStatusChange(bool running)
    {
        if (running)
        {
            _fixedFrameCount = 0;
            _update = true;
        }
        else
        {
            _update = false;
        }
    }

    protected override string Update()
    {
        return $"Fixed Frame: {_fixedFrameCount.ToString()}";
    }

    private void FixedUpdateActual()
    {
        if (!_update) return;
        _fixedFrameCount++;
    }
}