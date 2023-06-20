using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.Drawing;

public partial class Drawing
{
    private readonly Queue<PendingTexture> _textures = new();
    private readonly Action _nextTexture;

    public void DrawTexture(Vector2 offset, Texture2D texture)
    {
        _textures.Enqueue(new(texture, offset));
        _pendingDraws.Add(_nextTexture);
    }

    private void NextTexture()
    {
        var pendingTexture = _textures.Dequeue();
        var texture = pendingTexture.Texture;
        var offset = pendingTexture.Offset;

        Graphics.DrawTexture(new(offset.x, offset.y, texture.width, texture.height), pendingTexture.Texture);
    }

    private struct PendingTexture
    {
        public Texture2D Texture { get; }
        public Vector2 Offset { get; }

        public PendingTexture(Texture2D texture, Vector2 offset)
        {
            Texture = texture;
            Offset = offset;
        }
    }
}