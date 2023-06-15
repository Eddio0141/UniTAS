using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.DontRunIfPaused;
using UniTAS.Patcher.Interfaces.Events.Movie;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;

namespace UniTAS.Patcher.Implementations.GUI.Overlays;

[Singleton]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
public class FrameCountOverlay : BuiltInOverlay, IOnMovieRunningStatusChange, IOnFixedUpdateActual
{
    private uint _frameCount;
    private uint _fixedFrameCount;

    private bool _update;

    public FrameCountOverlay(IConfig config, IOverlayDrawing overlayDrawing) : base(config, overlayDrawing)
    {
    }

    protected override AnchoredOffset DefaultOffset => new(0, 0, 0, 0);
    protected override string ConfigValue => "FrameCount";

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

    protected override void Update()
    {
        if (_update)
        {
            _frameCount++;
        }

        Text = $"Frame: {_frameCount.ToString()}, Fixed Frame: {_fixedFrameCount.ToString()}";
    }

    public void FixedUpdateActual()
    {
        if (!_update) return;
        _fixedFrameCount++;
    }
}