using BepInEx;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Interfaces.GUI;

[ForceInstantiate]
public abstract class BuiltInOverlay : Window
{
    protected abstract AnchoredOffset DefaultOffset { get; }
    private const float SizeXExtra = 50;
    private const float SizeYExtra = 5;
    private const float MinXSize = 100;
    private const float MinYSize = 30;
    protected virtual int DefaultFontSize => 25;

    protected BuiltInOverlay(WindowDependencies windowDependencies, string windowId, bool showByDefault = true) : base(
        windowDependencies, windowId)
    {
        windowDependencies.UpdateEvents.OnUpdateUnconditional += UpdateUnconditional;
        Init(showByDefault);
    }

    private void Init(bool showByDefault)
    {
        Config = new(
            defaultWindowRect: new(DefaultOffset.X, DefaultOffset.Y, MinXSize, MinYSize), showByDefault: showByDefault);
        base.Init();
        NoWindowDuringToolBarHide = true;
        Resizable = false;
    }

    protected override void OnGUIWhileToolbarHide()
    {
        GUILayout.BeginArea(new Rect(5, 0, Screen.width, Screen.height));
        var size = GUIUtils.ShadowedText(WindowConfigId, DefaultFontSize, 0, 0);
        GUILayout.EndArea();

        FixWindowSize(Event.current, size);
    }

    protected override void OnGUI()
    {
        var currentEvent = Event.current;

        if (_text.IsNullOrWhiteSpace())
        {
            if (_showOverlay)
            {
                _pendingNewLayout = true;
                _showOverlay = false;
            }

            if (!_pendingNewLayout || currentEvent.type == EventType.Layout)
            {
                _pendingNewLayout = false;
                return;
            }
        }
        else
        {
            _showOverlay = true;
        }

        if (_showOverlay && _pendingNewLayout && currentEvent.type != EventType.Layout) return;
        if (_showOverlay)
            _pendingNewLayout = false;

        GUILayout.BeginArea(new Rect(5, 0, Screen.width, Screen.height));

        var size = GUIUtils.ShadowedText(_text, DefaultFontSize, 0, 0);

        GUILayout.EndArea();

        FixWindowSize(currentEvent, size);
    }

    private void FixWindowSize(Event currentEvent, Vector2 size)
    {
        if (currentEvent.type != EventType.Layout) return;

        size.x = Mathf.Max(size.x + SizeXExtra, MinXSize);
        size.y = Mathf.Max(size.y + SizeYExtra, MinYSize);

        var prevWindowRect = WindowRect;

        WindowRect = new(prevWindowRect.x, prevWindowRect.y, size.x, size.y);
    }

    private string _text;
    private bool _pendingNewLayout;
    private bool _showOverlay;

    public void UpdateUnconditional()
    {
        _text = Update();
    }

    /// <summary>
    /// Update that happens before the text is drawn
    /// </summary>
    /// <returns>Text to draw. Return null to skip drawing.</returns>
    protected abstract string Update();
}