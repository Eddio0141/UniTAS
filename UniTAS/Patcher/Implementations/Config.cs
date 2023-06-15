using System.IO;
using BepInEx;
using BepInEx.Configuration;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations;

[Singleton]
[ExcludeRegisterIfTesting]
public class Config : IConfig
{
    public ConfigFile ConfigFile { get; } = new(Path.Combine(Paths.ConfigPath, FILE_NAME), true);
    private const string FILE_NAME = "UniTAS.cfg";
    private readonly FileSystemWatcher _fileSystemWatcher;

    public Config(ILogger logger)
    {
        _fileSystemWatcher = new(Paths.ConfigPath, FILE_NAME)
        {
            NotifyFilter = NotifyFilters.LastWrite
        };
        _fileSystemWatcher.Changed += (_, _) =>
        {
            ConfigFile.Reload();
            logger.LogDebug("Config file reloaded");
        };
        _fileSystemWatcher.EnableRaisingEvents = true;
    }
}