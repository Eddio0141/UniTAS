using BepInEx.Configuration;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Interfaces.GUI;

public abstract class BuiltInOverlay : IOnUpdateUnconditional
{
    private bool Enabled { get; set; }
    private AnchoredOffset Offset { get; set; }
    protected abstract AnchoredOffset DefaultOffset { get; }

    private readonly ConfigFile _config;
    protected abstract string ConfigValue { get; }

    private readonly IOverlayDrawing _overlayDrawing;

    protected string Text { get; set; }

    protected BuiltInOverlay(IConfig config, IOverlayDrawing overlayDrawing)
    {
        _overlayDrawing = overlayDrawing;
        _config = config.ConfigFile;
        Init();
    }

    private void Init()
    {
        Offset = ConfigUtils.BindAnchoredOffset(_config, $"BuiltInOverlays.{ConfigValue}", DefaultOffset);
        Enabled = _config.Bind($"BuiltInOverlays.{ConfigValue}", "Enabled", true).Value;
    }

    public void UpdateUnconditional()
    {
        if (!Enabled) return;
        Update();
        _overlayDrawing.DrawText(Offset, Text);
    }

    /// <summary>
    /// Update that happens before the text is drawn
    /// </summary>
    protected abstract void Update();
}