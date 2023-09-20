using System;
using System.IO;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Overlay;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI;

[Singleton]
[ExcludeRegisterIfTesting]
public class CursorOverlay : IOnUpdateUnconditional, IMouseOverlayStatus, IOnGameRestart
{
    private readonly IDrawing _drawing;

    private readonly Texture2D _cursorTexture = new(1, 1, TextureFormat.ARGB32, false);
    private readonly bool _disabled;

    public bool Visible { private get; set; } = true;

    public CursorOverlay(IDrawing drawing, ILogger logger, ITextureWrapper textureWrapper)
    {
        _drawing = drawing;

        var imagePath = Path.Combine(UniTASPaths.Resources, "cursor.png");

        try
        {
            textureWrapper.LoadImage(_cursorTexture, imagePath);
            _cursorTexture.Apply();
        }
        catch (Exception e)
        {
            logger.LogError($"Failed to load cursor image\n{e}");
            _disabled = true;
        }
    }

    public void UpdateUnconditional()
    {
        if (_disabled || !Visible) return;
        var mousePos = Input.mousePosition;
        _drawing.DrawTexture(new(mousePos.x, Screen.height - mousePos.y), _cursorTexture);
    }

    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        Visible = true;
    }
}