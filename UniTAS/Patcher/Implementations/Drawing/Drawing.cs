using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Interfaces.GUI;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.Drawing;

[Singleton]
[ExcludeRegisterIfTesting]
public partial class Drawing : IOnGUIUnconditional, IDrawing
{
    private readonly List<Action> _pendingDraws = new();

    private int _cachedScreenWidth;
    private int _cachedScreenHeight;

    public Drawing()
    {
        _nextText = NextText;
        _nextTexture = NextTexture;
    }

    public void OnGUIUnconditional()
    {
        if (Event.current.type != EventType.Repaint) return;

        _cachedScreenWidth = Screen.width;
        _cachedScreenHeight = Screen.height;

        foreach (var pendingDraw in _pendingDraws)
        {
            pendingDraw();
        }

        _pendingDraws.Clear();
    }
}