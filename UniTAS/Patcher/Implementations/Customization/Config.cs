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

namespace UniTAS.Patcher.Implementations.Customization;

[Singleton]
[ExcludeRegisterIfTesting]
public partial class Config : IConfig, IDisposable
{
    public ConfigFile BepInExConfigFile { get; } = new(UniTASPaths.ConfigBepInEx, true);

    private readonly FileSystemWatcher _fileSystemWatcher;
    private readonly Timer _timer;

    private readonly Dictionary<string, string> _backendEntries = new();
    private const char EntrySeparator = '\u0D9E';

    private readonly ILogger _logger;

    public Config(ILogger logger)
    {
        _logger = logger;
        try
        {
            _fileSystemWatcher = new(Paths.ConfigPath, UniTASPaths.BepInExConfigFileName)
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
        BepInExConfigFile.Bind(Sections.Debug.FunctionCallTrace.SectionName, Sections.Debug.FunctionCallTrace.Enable,
            false,
            "If enabled, will hook on most functions and log every function call");
        BepInExConfigFile.Bind(Sections.Debug.FunctionCallTrace.SectionName,
            Sections.Debug.FunctionCallTrace.Methods, "",
            "A list of methods to hook call tracing to. Each entry must consist of `Type:method`. You can append to the list by separating each entry with a comma. Example: `UnityEngine.Time:get_captureDeltaTime, Game.HeadCrab:Update`");

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

            var separatorIndex = backendConfigEntry.IndexOf(EntrySeparator);
            if (separatorIndex < 0)
            {
                // invalid
                _logger.LogWarning(
                    $"Backend config has an invalid line at line {i}: {backendConfigEntry}, removing and saving config");
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
                        _logger.LogError(
                            $"Couldn't delete backend config, what??? Continuing with whatever is loaded: {e2}");
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

        WriteEntries();
    }

    public bool TryGetBackendEntry<T>(string key, out T value)
    {
        if (!_backendEntries.TryGetValue(key, out var entry))
        {
            value = default;
            return false;
        }

        try
        {
            value = JsonConvert.DeserializeObject<T>(entry);
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to deserialize key `{key}`, deleting entry: {e}");
            RemoveBackendEntry(key);
            value = default;
            return false;
        }

        return true;
    }

    public void RemoveBackendEntry(string key)
    {
        if (_backendEntries.Remove(key))
            WriteEntries();
    }

    private void WriteEntries()
    {
        var entries = _backendEntries.Select(x => $"{x.Key}{EntrySeparator}{x.Value}").ToArray();

        try
        {
            File.WriteAllLines(UniTASPaths.ConfigBackend, entries);
        }
        catch (Exception e)
        {
            _logger.LogWarning($"Failed to save backend config: {e}");
        }
    }
}