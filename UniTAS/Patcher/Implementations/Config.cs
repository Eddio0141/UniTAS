using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using BepInEx;
using BepInEx.Configuration;
using Newtonsoft.Json;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations;

[Singleton]
[ExcludeRegisterIfTesting]
public class Config : IConfig, IDisposable
{
    public ConfigFile BepInExConfigFile { get; } = new(UniTASPaths.ConfigBepInEx, true);

    private readonly FileSystemWatcher _fileSystemWatcher;
    private readonly Timer _timer;

    private readonly Dictionary<string, string> _backendEntries = new();
    private const char ENTRY_SEPARATOR = ':';

    private readonly ILogger _logger;

    public Config(ILogger logger)
    {
        _logger = logger;
        try
        {
            _fileSystemWatcher = new(Paths.ConfigPath, UniTASPaths.BEPINEX_CONFIG_FILE_NAME)
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
        BepInExConfigFile.Bind(Sections.Debug.FunctionCallTrace.SECTION_NAME, Sections.Debug.FunctionCallTrace.ENABLE, false,
            "If enabled, will hook on most functions and log every function call");
        BepInExConfigFile.Bind(Sections.Debug.FunctionCallTrace.SECTION_NAME, Sections.Debug.FunctionCallTrace.MATCHING_TYPES, "*",
            "A list of glob pattern of types to hook function call tracing to. You can append to the list by separating each entry with a comma. Example: `UnityEngine.Application, UnityEngine.Time, UnityEngine.InputSystem.*`");

        var backendConfigRaw = new List<string>();
        try
        {
            backendConfigRaw = File.ReadAllLines(UniTASPaths.ConfigBackend).ToList();
        }
        catch (Exception e)
        {
            _logger.LogInfo($"Couldn't read backend config file: {e}");
        }

        for (var i = backendConfigRaw.Count - 1; i > -1; i--)
        {
            var backendConfigEntry = backendConfigRaw[i];

            var separatorIndex = backendConfigEntry.IndexOf(ENTRY_SEPARATOR);
            if (separatorIndex < 0)
            {
                // invalid
                _logger.LogWarning($"Backend config has an invalid line at line {i}: {backendConfigEntry}, removing and saving config");
                backendConfigRaw.RemoveAt(i);

                try
                {
                    File.WriteAllLines(UniTASPaths.ConfigBackend, backendConfigRaw.ToArray());
                }
                catch (Exception e)
                {
                    _logger.LogWarning($"Couldn't write to backend config for some reason: {e}");

                    try
                    {
                        File.Delete(UniTASPaths.ConfigBackend);
                    }
                    catch (Exception e2)
                    {
                        _logger.LogError($"Couldn't delete backend config, what??? Continuing with whatever is loaded: {e2}");
                    }
                }

                continue;
            }

            var key = backendConfigEntry.Substring(0, separatorIndex);
            var value = backendConfigEntry.Substring(separatorIndex + 1);

            _backendEntries.Add(key, value);
        }
    }

    private void ConfigReload(bool logReload)
    {
        BepInExConfigFile.Reload();
        if (logReload)
        {
            _logger.LogDebug("Config file reloaded");
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public void WriteBackendEntry<T>(string key, T value)
    {
        var entry = JsonConvert.SerializeObject(value);
        _backendEntries[key] = entry;

        var entries = _backendEntries.Select(x => $"{x.Key}{ENTRY_SEPARATOR}{x.Value}").ToArray();

        try
        {
            File.WriteAllLines(UniTASPaths.ConfigBackend, entries);
        }
        catch (Exception e)
        {
            _logger.LogWarning($"Failed to save backend config for entry {key}: {e}");
        }
    }

    public bool TryGetBackendEntry<T>(string key, out T value)
    {
        if (!_backendEntries.TryGetValue(key, out var entry))
        {
            value = default;
            return false;
        }

        value = JsonConvert.DeserializeObject<T>(entry);
        return true;
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
