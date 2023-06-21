using System;
using System.IO;
using System.Threading;
using BepInEx;
using BepInEx.Configuration;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations;

[Singleton]
[ExcludeRegisterIfTesting]
public class Config : IConfig, IDisposable
{
    public ConfigFile ConfigFile { get; } = new(Path.Combine(Paths.ConfigPath, FILE_NAME), true);
    private const string FILE_NAME = "UniTAS.cfg";

    private readonly FileSystemWatcher _fileSystemWatcher;
    private readonly Timer _timer;

    private readonly ILogger _logger;

    public Config(ILogger logger)
    {
        _logger = logger;
        try
        {
            _fileSystemWatcher = new(Paths.ConfigPath, FILE_NAME)
            {
                NotifyFilter = NotifyFilters.LastWrite
            };
            _fileSystemWatcher.Changed += (_, _) => { ConfigReload(); };
            _fileSystemWatcher.EnableRaisingEvents = true;
        }
        catch (Exception)
        {
            _logger.LogDebug("Failed to create FileSystemWatcher, using timer instead");

            // unity being stupid and hasn't implemented any classes in FileSystemWatcher
            _timer = new(_ => { ConfigReload(); }, null, 0, 1000);
        }
    }

    private void ConfigReload()
    {
        ConfigFile.Reload();
        _logger.LogDebug("Config file reloaded");
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}