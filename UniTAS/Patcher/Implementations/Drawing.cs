using System.Collections.Generic;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

[Singleton]
[ExcludeRegisterIfTesting]
public class Drawing : IOnGUIUnconditional, IDrawing
{
    private readonly Texture2D _texture = new(Screen.width, Screen.height, TextureFormat.ARGB32, false);
    private readonly Color32[] _pixels = new Color32[Screen.width * Screen.height];
    private readonly List<PendingText> _texts = new();

    private bool _textureDirty = true;

    private readonly GUIStyle _labelWithFontSize = new();
    private readonly GUIContent _labelContent = new();

    public void FillBox(AnchoredOffset offset, int width, int height, Color32 color)
    {
        var screenWidth = Screen.width;

        var x = offset.X;
        var y = offset.Y;
        var destX = x + width;
        var destY = y + height;

        for (var posX = x; posX < destX; posX++)
        {
            for (var posY = y; posY < destY; posY++)
            {
                var index = posY * screenWidth + posX;
                if (!_textureDirty && !_pixels[index].EqualsColor(color)) _textureDirty = true;
                _pixels[index] = color;
            }
        }
    }

    public void PrintText(AnchoredOffset offset, string text, int fontSize)
    {
        _texts.Add(new(text, offset, fontSize));
    }

    private const int TEXT_SHADOW_OFFSET = 1;

    private static readonly TupleValue<int, int>[] TextShadowOffsets =
    {
        new(TEXT_SHADOW_OFFSET, TEXT_SHADOW_OFFSET), new(-TEXT_SHADOW_OFFSET * 2, 0),
        new(0, -TEXT_SHADOW_OFFSET * 2), new(TEXT_SHADOW_OFFSET * 2, 0)
    };

    public void OnGUIUnconditional()
    {
        if (Event.current.type != EventType.Repaint) return;

        if (_textureDirty)
        {
            _textureDirty = false;
            _texture.SetPixels32(_pixels);
            _texture.Apply();
        }

        Graphics.DrawTexture(new(0, 0, Screen.width, Screen.height), _texture);

        foreach (var text in _texts)
        {
            _labelWithFontSize.fontSize = text.FontSize;
            _labelWithFontSize.normal.textColor = Color.black;
            _labelContent.text = text.Text;

            var size = _labelWithFontSize.CalcSize(_labelContent);
            var offset = text.Offset;

            var posX = offset.X - offset.AnchorX * size.x;
            var posY = offset.Y - offset.AnchorY * size.y;

            var labelRect = new Rect(posX, posY, size.x, size.y);
            var labelRectOriginal = new Rect(labelRect);

            foreach (var shadowOffset in TextShadowOffsets)
            {
                labelRect.x += shadowOffset.Item1;
                labelRect.y += shadowOffset.Item2;
                UnityEngine.GUI.Label(labelRect, text.Text, _labelWithFontSize);
            }

            _labelWithFontSize.normal.textColor = Color.white;
            UnityEngine.GUI.Label(labelRectOriginal, text.Text, _labelWithFontSize);
        }

        _texts.Clear();
    }

    private struct PendingText
    {
        public string Text { get; }
        public AnchoredOffset Offset { get; }
        public int FontSize { get; }

        public PendingText(string text, AnchoredOffset offset, int fontSize)
        {
            Text = text;
            Offset = offset;
            FontSize = fontSize;
        }
    }
}