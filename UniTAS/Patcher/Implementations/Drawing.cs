using System.Collections.Generic;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Interfaces.GUI;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

[Singleton]
public class Drawing : IOnGUIUnconditional, IDrawing
{
    private readonly Texture2D _texture = new(Screen.width, Screen.height, TextureFormat.ARGB32, false);
    private readonly Color32[] _pixels = new Color32[Screen.width * Screen.height];
    private readonly List<PendingText> _texts = new();

    private bool _textureDirty = true;

    public void FillBox(int x, int y, int width, int height, Color32 color)
    {
        var screenWidth = Screen.width;

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

    public void PrintText(int x, int y, string text)
    {
        var width = text.Length * 10;
        const int height = 20;
        _texts.Add(new(text, new(x, y, width, height)));
    }

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
            UnityEngine.GUI.Label(text.Rect, text.Text);
        }

        _texts.Clear();
    }

    private struct PendingText
    {
        public string Text { get; }
        public Rect Rect { get; }

        public PendingText(string text, Rect rect)
        {
            Text = text;
            Rect = rect;
        }
    }
}