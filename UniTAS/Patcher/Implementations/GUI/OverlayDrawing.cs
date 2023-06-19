using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;

namespace UniTAS.Patcher.Implementations.GUI;

[Singleton]
public class OverlayDrawing : IOverlayDrawing
{
    private readonly IDrawing _drawing;

    public OverlayDrawing(IDrawing drawing)
    {
        _drawing = drawing;
    }

    public void DrawText(AnchoredOffset offset, string text, int fontSize)
    {
        _drawing.PrintText(offset, text, fontSize);
    }
}