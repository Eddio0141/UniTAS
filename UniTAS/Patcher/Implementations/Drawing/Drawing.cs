using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.Drawing;

[Singleton]
[ExcludeRegisterIfTesting]
public partial class Drawing : IOnGUIUnconditional, IDrawing
{
    private readonly List<Action> _pendingDraws = new();

    public Drawing()
    {
        _nextText = NextText;
        _nextTexture = NextTexture;
    }

    public void OnGUIUnconditional()
    {
        if (Event.current.type != EventType.Repaint) return;

        foreach (var pendingDraw in _pendingDraws)
        {
            pendingDraw();
        }

        _pendingDraws.Clear();
    }

    private static Vector2 GetScreenPosition(AnchoredOffset offset, Vector2 size)
    {
        var posX = offset.X - offset.AnchorX * size.x;
        var posY = offset.Y - offset.AnchorY * size.y;

        return new(posX, posY);
    }
}