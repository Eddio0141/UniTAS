using BepInEx.Configuration;

namespace UniTAS.Patcher.Services;

public interface IConfig
{
    /// <summary>
    /// BepInEx config file for user end config
    /// </summary>
    ConfigFile BepInExConfigFile { get; }

    /// <summary>
    /// Tries to get config entry for backend config
    /// </summary>
    bool TryGetBackendEntry<T>(string key, out T value);

    /// <summary>
    /// Write or update config entry for backend config
    /// </summary>
    void WriteBackendEntry<T>(string key, T value);
}
