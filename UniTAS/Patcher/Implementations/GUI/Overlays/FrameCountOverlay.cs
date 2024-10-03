using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.Movie;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.DontRunIfPaused;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;

namespace UniTAS.Patcher.Implementations.GUI.Overlays;

[ForceInstantiate]
[Singleton]
[ExcludeRegisterIfTesting]
public class FrameCountOverlay : BuiltInOverlay, IOnMovieRunningStatusChange, IOnFixedUpdateActual
{
    private uint _frameCount;
    private uint _fixedFrameCount;

    private bool _update;

    public FrameCountOverlay(IConfig config, IDrawing drawing) : base(config, drawing)
    {
    }

    protected override AnchoredOffset DefaultOffset => new(0, 0, 0, 0);
    protected override string ConfigName => "FrameCount";

    public void OnMovieRunningStatusChange(bool running)
    {
        if (running)
        {
            _frameCount = 0;
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
        if (_update)
        {
            _frameCount++;
        }

        return $"Frame: {_frameCount.ToString()}, Fixed Frame: {_fixedFrameCount.ToString()}";
    }

    public void FixedUpdateActual()
    {
        if (!_update) return;
        _fixedFrameCount++;
    }
}