using UnityEngine;

namespace UniTAS.Patcher.Interfaces.GUI;

public interface IDrawing
{
    void FillBox(int x, int y, int width, int height, Color32 color);
    void PrintText(int x, int y, string text, int fontSize);
}