using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.Movie;
using UniTAS.Patcher.Services.Overlay;

namespace UniTAS.Patcher.Implementations.Movie.Engine.Modules;

public class GUI : EngineMethodClass
{
    private readonly IOverlayVisibleToggle _overlayVisibleToggle;

    [MoonSharpHidden]
    public GUI(IOverlayVisibleToggle overlayVisibleToggle)
    {
        _overlayVisibleToggle = overlayVisibleToggle;
    }

    public bool ShowOverlays
    {
        get => _overlayVisibleToggle.Enabled;
        set => _overlayVisibleToggle.Enabled = value;
    }
}