using UniTAS.Patcher.Models.GUI;

namespace UniTAS.Patcher.Interfaces.GUI;

public interface IOverlayDrawing
{
    void DrawText(AnchoredOffset offset, string text);
}