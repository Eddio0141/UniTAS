using BepInEx.Configuration;

namespace UniTAS.Patcher.Services;

public interface IConfig
{
    ConfigFile ConfigFile { get; }
}