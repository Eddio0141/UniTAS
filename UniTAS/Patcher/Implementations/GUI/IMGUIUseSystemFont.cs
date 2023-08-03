using System.Linq;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.EventSubscribers;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI;

[Register]
[ForceInstantiate]
[ExcludeRegisterIfTesting]
public class IMGUIUseSystemFont
{
    private readonly IUpdateEvents _updateEvents;
    private readonly Font _font;
    private readonly ILogger _logger;

    private const string FALLBACK_FONT_NAME = "Liberation Sans";

    public IMGUIUseSystemFont(ILogger logger, IUpdateEvents updateEvents)
    {
        _logger = logger;
        _updateEvents = updateEvents;
        var fonts = ResourcesUtils.FindObjectsOfTypeAll<Font>();
        if (fonts.Length == 0)
        {
            logger.LogWarning("no fallback font found for unity IMGUI");
            return;
        }

        _font = fonts.FirstOrDefault(x => x.fontNames.Contains(FALLBACK_FONT_NAME));
        if (_font == null)
        {
            logger.LogError($"couldn't find unity fallback font {FALLBACK_FONT_NAME}");
            return;
        }

        _updateEvents.OnGUIEventUnconditional += OnGUIUnconditional;
    }

    private void OnGUIUnconditional()
    {
        _updateEvents.OnGUIEventUnconditional -= OnGUIUnconditional;
        _logger.LogDebug(UnityEngine.GUI.skin.font.fontNames.Join());
        UnityEngine.GUI.skin.font = _font;
        _logger.LogInfo($"Set unity IMGUI font to {FALLBACK_FONT_NAME}");
    }
}