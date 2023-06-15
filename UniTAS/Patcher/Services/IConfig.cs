using BepInEx.Configuration;

namespace UniTAS.Patcher.Services;

public interface IConfig
{
    float DefaultFps { get; set; }
    ConfigFile ConfigFile { get; }
}