using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services.Movie;

namespace UniTAS.Patcher.Implementations.GUI.Overlays;

[Singleton]
[ExcludeRegisterIfTesting]
public class FixedFrameCountOverlay : BuiltInOverlay
{
    private uint _fixedFrameCount;

    private bool _update;

    public FixedFrameCountOverlay(WindowDependencies windowDependencies, IMovieRunner movieRunner) : base(
        windowDependencies, "Fixed frames count", showByDefault: false)
    {
        windowDependencies.UpdateEvents.OnFixedUpdateActual += FixedUpdateActual;
        movieRunner.OnMovieStart += () =>
        {
            _fixedFrameCount = 0;
            _update = true;
        };
        movieRunner.OnMovieEnd += () => { _update = false; };
    }

    protected override AnchoredOffset DefaultOffset => new(0, 0, 200, 30);

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