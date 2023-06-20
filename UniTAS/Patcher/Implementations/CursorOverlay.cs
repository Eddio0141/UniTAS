using System;
using System.IO;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Overlay;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

[Singleton]
public class CursorOverlay : IOnUpdateUnconditional, IMouseOverlayStatus, IOnGameRestart
{
    private readonly IDrawing _drawing;
    private readonly Texture2D _cursorTexture = new(1, 1);
    private readonly bool _disabled;

    public bool Visible { private get; set; }

    public CursorOverlay(IDrawing drawing, ILogger logger)
    {
        _drawing = drawing;
        var imagePath = Path.Combine(UniTASPaths.Resources, "cursor.png");
        if (!File.Exists(imagePath))
        {
            logger.LogError($"Cursor image not found at {imagePath}");
            _disabled = true;
            return;
        }

        var cursorBytes = File.ReadAllBytes(imagePath);

        var textureLoadImage = AccessTools.Method(typeof(Texture2D), "LoadImage");
        if (textureLoadImage == null)
        {
            var textureLoadRawTextureData =
                AccessTools.Method(typeof(Texture2D), "LoadRawTextureData", new[] { typeof(byte[]) });
            if (textureLoadRawTextureData == null)
            {
                logger.LogError("Failed to find LoadImage or LoadRawTextureData");
                _disabled = true;
                return;
            }

            textureLoadRawTextureData.Invoke(_cursorTexture, new object[] { cursorBytes });
        }
        else
        {
            if (!(bool)textureLoadImage.Invoke(_cursorTexture, new object[] { cursorBytes }))
            {
                logger.LogError("Failed to load cursor image");
                _disabled = true;
                return;
            }
        }

        _cursorTexture.Apply();
    }

    public void UpdateUnconditional()
    {
        if (_disabled || !Visible) return;
        var mousePos = Input.mousePosition;
        _drawing.DrawTexture(new(mousePos.x, mousePos.y), _cursorTexture);
    }

    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        Visible = true;
    }
}