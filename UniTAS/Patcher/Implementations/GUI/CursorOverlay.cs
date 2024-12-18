using System;
using System.IO;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Overlay;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI;

[Singleton]
[ExcludeRegisterIfTesting]
public class CursorOverlay : IOnUpdateUnconditional, IMouseOverlayStatus, IOnGameRestart, IOnGUIUnconditional
{
    private readonly Texture2D _cursorTexture = new(1, 1, TextureFormat.ARGB32, false);
    private readonly bool _disabled;
    private readonly IUnityInputWrapper _inputWrapper;

    public bool Visible { private get; set; } = true;

    public CursorOverlay(ILogger logger, ITextureWrapper textureWrapper, IUnityInputWrapper inputWrapper)
    {
        _inputWrapper = inputWrapper;
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

    private Vector3 _lastPosition;

    public void UpdateUnconditional()
    {
        _lastPosition = _inputWrapper.GetMousePosition(false);
    }

    public void OnGUIUnconditional()
    {
        if (_disabled || !Visible) return;
        UnityEngine.GUI.DrawTexture(
            new(_lastPosition.x, Screen.height - _lastPosition.y, _cursorTexture.width, _cursorTexture.height),
            _cursorTexture);
    }

    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        Visible = true;
    }
}