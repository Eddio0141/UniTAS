using UniTAS.Patcher.Models.GUI;
using UnityEngine;

namespace UniTAS.Patcher.Interfaces.GUI;

public interface IDrawing
{
    void PrintText(AnchoredOffset offset, string text, int fontSize);
    void DrawTexture(Vector2 pos, Texture2D texture);
}