using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UniTAS.Patcher.Implementations.Customization;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Localization;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Utils;

[Singleton(RegisterPriority.ToolBar)]
[ForceInstantiate]
public class Localization : ILocalization
{
    public Dictionary<string, Dictionary<string, string>> Tables { get; set; }
    public string Locale { get; set; } = "en";

    private readonly IConfig _config;
    private readonly ILogger _logger;

    public Localization(IConfig config, ILogger logger)
    {
        _config = config;
        _logger = logger;
        try
        {
            Locale = _config.TryGetBackendEntry(Config.Sections.Localization.Locale, out string locale) ? locale : "en";
            Load(Locale, UniTASPaths.Localization);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to load localization file: {ex.Message}. Keys will be displayed.\r\n{ex.StackTrace}");
            Tables = new Dictionary<string, Dictionary<string, string>>();
        }
    }

    public void Load(string locale, string path)
    {
        Locale = locale;
        Tables = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(path));
    }

    public string Get(string key)
    {
        if (Tables.TryGetValue(Locale, out var table) && table.TryGetValue(key, out var value))
            return value;
        if (Tables["en"].TryGetValue(key, out var enValue))
            return enValue;
        return $"#{key}#";
    }
}