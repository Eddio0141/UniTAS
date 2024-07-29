using System;
using System.IO;
using System.Threading;
using BepInEx;
using BepInEx.Configuration;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations;

[Singleton]
[ExcludeRegisterIfTesting]
public class Config : IConfig, IDisposable
{
    public ConfigFile ConfigFile { get; } = new(UniTASPaths.Config, true);

    private readonly FileSystemWatcher _fileSystemWatcher;
    private readonly Timer _timer;

    private readonly ILogger _logger;

    public Config(ILogger logger)
    {
        _logger = logger;
        try
        {
            _fileSystemWatcher = new(Paths.ConfigPath, UniTASPaths.CONFIG_FILE_NAME)
            {
                NotifyFilter = NotifyFilters.LastWrite
            };
            _fileSystemWatcher.Changed += (_, _) => { ConfigReload(true); };
            _fileSystemWatcher.EnableRaisingEvents = true;
        }
        catch (Exception)
        {
            _logger.LogDebug("Failed to create FileSystemWatcher, using timer instead");

            // unity being stupid and hasn't implemented any classes in FileSystemWatcher
            _timer = new(_ => { ConfigReload(false); }, null, 0, 1000);
        }

        // add entry for patcher config entry
        ConfigFile.Bind(Sections.Debug.FunctionCallTrace.SECTION_NAME, Sections.Debug.FunctionCallTrace.ENABLE, false,
            "If enabled, will hook on most functions and log every function call");
        ConfigFile.Bind(Sections.Debug.FunctionCallTrace.SECTION_NAME, Sections.Debug.FunctionCallTrace.MATCHING_TYPES, "*",
            "A list of glob pattern of types to hook function call tracing to. You can append to the list by separating each entry with a comma. Example: \"UnityEngine.Application, UnityEngine.Time, UnityEngine.InputSystem.*\"");
    }

    private void ConfigReload(bool logReload)
    {
        ConfigFile.Reload();
        if (logReload)
        {
            _logger.LogDebug("Config file reloaded");
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public static class Sections
    {
        public static class Debug
        {
            public static class FunctionCallTrace
            {
                public const string SECTION_NAME = $"{nameof(Debug)}.FunctionCallTrace";
                public const string ENABLE = "Enable";
                public const string MATCHING_TYPES = "MatchingTypes";
            }
        }
    }
}
