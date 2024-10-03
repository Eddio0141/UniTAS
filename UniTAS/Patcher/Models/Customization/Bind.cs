using System;
using System.Diagnostics.CodeAnalysis;
using BepInEx.Configuration;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;

namespace UniTAS.Patcher.Models.Customization;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class Bind
{
    public KeyCode Key
    {
        get
        {
            var configEntry = _keyConfigEntry?.Value;
            if (configEntry != null && _key.ToString() != configEntry && Enum.IsDefined(typeof(KeyCode), configEntry))
            {
                _key = (KeyCode)Enum.Parse(typeof(KeyCode), configEntry);
            }

            return _key;
        }
    }

    public string Name { get; }
    private ConfigEntry<string> _keyConfigEntry;

    private readonly IConfig _config;
    private readonly IUnityInputWrapper _unityInput;
    private KeyCode _key;

    private const string CONFIG_SECTION = "Binds";

    public Bind(BindConfig bindConfig, IConfig config, IUnityInputWrapper unityInput)
    {
        if (bindConfig == null) throw new ArgumentNullException(nameof(bindConfig));
        _key = bindConfig.Key;
        _config = config;
        _unityInput = unityInput;
        Name = bindConfig.Name;
    }

    private bool _initialized;

    public void InitConfig(bool noGenConfig)
    {
        if (_initialized || noGenConfig) return;
        _initialized = true;
        _keyConfigEntry = _config.BepInExConfigFile.Bind(CONFIG_SECTION, Name, _key.ToString(),
            "Key to press to trigger this bind. See https://docs.unity3d.com/ScriptReference/KeyCode.html for a list of keys.");
    }

    public bool IsPressed()
    {
        return _unityInput.GetKeyDown(Key);
    }
}

public class BindConfig(string name, KeyCode key)
{
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
    public KeyCode Key { get; } = key;
}