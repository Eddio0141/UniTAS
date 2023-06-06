using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

[Singleton]
public class Drawing : IOnUpdateUnconditional, IOnAwakeUnconditional, IOnGUIUnconditional
{
    private RenderTexture _renderTexture;
    private Texture2D _texture;
    private Color32[] _pixels;
    private bool _initialized;

    private bool _dirty;

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
                if (!_dirty && !_pixels[index].EqualsColor(color)) _dirty = true;
                _pixels[index] = color;
            }
        }
    }

    public void AwakeUnconditional()
    {
        if (_initialized) return;
        _initialized = true;

        _renderTexture = new(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        _renderTexture.Create();
        _texture = new(Screen.width, Screen.height, TextureFormat.ARGB32, false);
        _pixels = new Color32[Screen.width * Screen.height];
        _dirty = true;
    }

    public void UpdateUnconditional()
    {
        FillBox(0, 0, 200, 200, Color.red);
    }

    public void OnGUIUnconditional()
    {
        if (Event.current.type != EventType.Repaint) return;

        if (_dirty)
        {
            _dirty = false;
            _texture.SetPixels32(_pixels);
            _texture.Apply();
        }

        Graphics.DrawTexture(new(0, 0, Screen.width, Screen.height), _texture);
    }
}