using BepInEx.Configuration;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Overlay;

namespace UniTAS.Patcher.Interfaces.GUI;

public abstract class BuiltInOverlay : IOnUpdateUnconditional, IOverlayVisibleToggle
{
    protected abstract AnchoredOffset DefaultOffset { get; }
    protected virtual int DefaultFontSize => 25;

    private ConfigEntry<int> _anchorX;
    private ConfigEntry<int> _anchorY;
    private ConfigEntry<int> _offsetX;
    private ConfigEntry<int> _offsetY;
    private ConfigEntry<bool> _enabled;
    private ConfigEntry<int> _fontSize;

    protected abstract string ConfigName { get; }

    private readonly IDrawing _drawing;

    public bool Enabled { get; set; } = true;

    protected BuiltInOverlay(IConfig config, IDrawing drawing)
    {
        _drawing = drawing;
        Init(config);
    }

    private void Init(IConfig config)
    {
        var entry = $"BuiltInOverlays.{ConfigName}";
        _anchorX = config.ConfigFile.Bind(entry, "AnchorX", DefaultOffset.AnchorX,
            "Anchor X position. 0 is left, 1 is right.");
        _anchorY = config.ConfigFile.Bind(entry, "AnchorY", DefaultOffset.AnchorY,
            "Anchor Y position. 0 is top, 1 is bottom.");
        _offsetX = config.ConfigFile.Bind(entry, "OffsetX", DefaultOffset.OffsetX, "Offset X position.");
        _offsetY = config.ConfigFile.Bind(entry, "OffsetY", DefaultOffset.OffsetY, "Offset Y position.");
        _enabled = config.ConfigFile.Bind(entry, "Enabled", true);
        _fontSize = config.ConfigFile.Bind(entry, "FontSize", DefaultFontSize);
    }

    public void UpdateUnconditional()
    {
        if (!Enabled || !_enabled.Value) return;
        var text = Update();
        if (text == null) return;
        _drawing.PrintText(new(_anchorX.Value, _anchorY.Value, _offsetX.Value, _offsetY.Value), text, _fontSize.Value);
    }

    /// <summary>
    /// Update that happens before the text is drawn
    /// </summary>
    /// <returns>Text to draw. Return null to skip drawing.</returns>
    protected abstract string Update();
}