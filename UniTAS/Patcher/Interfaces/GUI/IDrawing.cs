using UniTAS.Patcher.Models.GUI;
using UnityEngine;

namespace UniTAS.Patcher.Interfaces.GUI;

public interface IDrawing
{
    void FillBox(AnchoredOffset offset, int width, int height, Color32 color);
    void PrintText(AnchoredOffset offset, string text, int fontSize);
}