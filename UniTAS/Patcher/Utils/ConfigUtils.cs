using BepInEx.Configuration;
using UniTAS.Patcher.Models.GUI;

namespace UniTAS.Patcher.Utils;

public static class ConfigUtils
{
    /// <summary>
    /// Binds the <see cref="AnchoredOffset"/> instance to the config
    /// </summary>
    /// <param name="config">The config file</param>
    /// <param name="section">Section to append the config to. So if the entry is at "General.Offset", the entry for this type would be split into "General.Offset -> AnchorX", "AnchorY", etc</param>
    /// <param name="defaultValue">Default value to fallback to</param>
    public static AnchoredOffset BindAnchoredOffset(ConfigFile config, string section, AnchoredOffset defaultValue)
    {
        var readAnchoredOffset = new AnchoredOffset(config.Bind(section, nameof(AnchoredOffset.AnchorX),
                defaultValue.AnchorX,
                "Anchor X position. 0 is left, 1 is right.").Value,
            config.Bind(section, nameof(AnchoredOffset.AnchorY), defaultValue.AnchorY,
                "Anchor Y position. 0 is top, 1 is bottom.").Value,
            config.Bind(section, nameof(AnchoredOffset.OffsetX), defaultValue.OffsetX,
                "Offset X position.").Value,
            config.Bind(section, nameof(AnchoredOffset.OffsetY), defaultValue.OffsetY,
                "Offset Y position.").Value
        );

        return readAnchoredOffset;
    }
}