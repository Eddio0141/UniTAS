using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Interfaces.Movie;

namespace UniTAS.Patcher.Implementations.Movie.Engine.Modules;

public class GUI : EngineMethodClass
{
    private readonly IOverlayDrawing _overlayDrawing;

    [MoonSharpHidden]
    public GUI(IOverlayDrawing overlayDrawing)
    {
        _overlayDrawing = overlayDrawing;
    }

    public bool ShowOverlays
    {
        get => _overlayDrawing.Enabled;
        set => _overlayDrawing.Enabled = value;
    }
}