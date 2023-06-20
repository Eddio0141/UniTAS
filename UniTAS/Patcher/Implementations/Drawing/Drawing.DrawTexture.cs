using System;
using System.Collections.Generic;
using UniTAS.Patcher.Models.GUI;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.Drawing;

public partial class Drawing
{
    private readonly Queue<Texture2D> _textures = new();
    private readonly Action _nextTexture;

    public void DrawTexture(AnchoredOffset offset, Texture2D texture)
    {
        _textures.Enqueue(texture);
        _pendingDraws.Add(_nextTexture);
    }

    private void NextTexture()
    {
        var texture = _textures.Dequeue();

        Graphics.DrawTexture(new(0, 0, _cachedScreenWidth, _cachedScreenHeight), texture);
    }
}