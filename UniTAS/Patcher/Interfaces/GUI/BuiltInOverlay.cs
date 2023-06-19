using BepInEx.Configuration;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;

namespace UniTAS.Patcher.Interfaces.GUI;

public abstract class BuiltInOverlay : IOnUpdateUnconditional
{
    protected abstract AnchoredOffset DefaultOffset { get; }

    private ConfigEntry<int> _anchorX;
    private ConfigEntry<int> _anchorY;
    private ConfigEntry<int> _offsetX;
    private ConfigEntry<int> _offsetY;
    private ConfigEntry<bool> _enabled;
    private ConfigEntry<int> _fontSize;

    protected abstract string ConfigValue { get; }

    private readonly IOverlayDrawing _overlayDrawing;

    protected string Text { get; set; }

    protected BuiltInOverlay(IConfig config, IOverlayDrawing overlayDrawing)
    {
        _overlayDrawing = overlayDrawing;
        Init(config);
    }

    private void Init(IConfig config)
    {
        var entry = $"BuiltInOverlays.{ConfigValue}";
        _anchorX = config.ConfigFile.Bind(entry, "AnchorX", DefaultOffset.AnchorX,
            "Anchor X position. 0 is left, 1 is right.");
        _anchorY = config.ConfigFile.Bind(entry, "AnchorY", DefaultOffset.AnchorY,
            "Anchor Y position. 0 is top, 1 is bottom.");
        _offsetX = config.ConfigFile.Bind(entry, "OffsetX", DefaultOffset.OffsetX, "Offset X position.");
        _offsetY = config.ConfigFile.Bind(entry, "OffsetY", DefaultOffset.OffsetY, "Offset Y position.");
        _enabled = config.ConfigFile.Bind(entry, "Enabled", true);
        _fontSize = config.ConfigFile.Bind(entry, "FontSize", 25);
    }

    public void UpdateUnconditional()
    {
        if (!_enabled.Value) return;
        Update();
        _overlayDrawing.DrawText(new(_anchorX.Value, _anchorY.Value, _offsetX.Value, _offsetY.Value), Text,
            _fontSize.Value);
    }

    /// <summary>
    /// Update that happens before the text is drawn
    /// </summary>
    protected abstract void Update();
}