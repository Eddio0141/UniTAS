using BepInEx.Configuration;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Services;

namespace UniTAS.Plugin.Implementations;

[Singleton]
[ExcludeRegisterIfTesting]
public class Config : IConfig
{
    private readonly ConfigEntry<float> _defaultFps;

    public Config(ConfigFile configFile)
    {
        _defaultFps = configFile.Bind("General", "DefaultFps", 100f,
            "Default FPS when the TAS isn't running. Make sure the FPS is more than 0");

        if (_defaultFps.Value <= 0)
        {
            _defaultFps.Value = 100f;
        }
    }

    public float DefaultFps
    {
        get => _defaultFps.Value;
        set => _defaultFps.Value = value;
    }
}