using System.Linq;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

[Singleton]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
public class IMGUIFontBackupSet
{
    private readonly IUpdateEvents _updateEvents;
    private readonly Font _font;
    private readonly ILogger _logger;

    public IMGUIFontBackupSet(ILogger logger, IUpdateEvents updateEvents)
    {
        _logger = logger;
        _updateEvents = updateEvents;
        var fonts = ResourcesUtils.FindObjectsOfTypeAll<Font>();
        if (fonts.Length == 0)
        {
            logger.LogWarning("no fallback font found for unity IMGUI");
            return;
        }

        foreach (var font in fonts)
        {
            _logger.LogDebug($"found font: {string.Join(", ", font.fontNames)}");
        }

        _font = fonts.FirstOrDefault(x => x.fontNames.Contains("Liberation Sans"));
        if (_font == null)
        {
            logger.LogWarning("no fallback font found for unity IMGUI");
            return;
        }

        _updateEvents.OnGUIUnconditional += OnGUIUnconditional;
    }

    private void OnGUIUnconditional()
    {
        _updateEvents.OnGUIUnconditional -= OnGUIUnconditional;
        UnityEngine.GUI.skin.font = _font;
        _logger.LogInfo($"Set unity IMGUI font to {_font.name}");
    }
}