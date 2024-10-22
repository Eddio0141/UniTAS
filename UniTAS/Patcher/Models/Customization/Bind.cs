using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;

namespace UniTAS.Patcher.Models.Customization;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class Bind
{
    public KeyCode Key
    {
        get => _key;
        set
        {
            if (_key == value) return;
            if (_key == KeyCode.None) throw new ArgumentException($"Key {value} is not a valid key.", nameof(value));
            _key = value;
            if (NoGenConfig) return;

            _config.WriteBackendEntry(_configEntry, value);
        }
    }

    public string Name { get; }
    public BindCategory Category { get; }
    private static readonly List<string> UsedNames = new();
    private string _configEntry;

    private readonly IConfig _config;
    private readonly IUnityInputWrapper _unityInput;
    private KeyCode _key;

    private const string ConfigPrefix = "Binds";

    public Bind(BindConfig bindConfig, IConfig config, IUnityInputWrapper unityInput)
    {
        if (bindConfig == null) throw new ArgumentNullException(nameof(bindConfig));
        _key = bindConfig.Key;
        Category = bindConfig.Category;
        _config = config;
        _unityInput = unityInput;
        Name = bindConfig.Name;
    }

    private bool _initialized;
    public bool NoGenConfig { get; private set; }

    public void InitConfig(bool noGenConfig)
    {
        if (_initialized || noGenConfig)
        {
            NoGenConfig = noGenConfig;
            _initialized = true;
            return;
        }

        _initialized = true;

        if (UsedNames.Contains(Name))
        {
            throw new Exception($"Duplicate bind name: {Name}");
        }

        UsedNames.Add(Name);

        _configEntry = $"{ConfigPrefix}-{Name}";
        if (_config.TryGetBackendEntry(_configEntry, out KeyCode key))
        {
            _key = key;
        }
        else
        {
            _config.WriteBackendEntry(_configEntry, _key);
        }
    }

    public bool IsPressed()
    {
        return _unityInput.GetKeyDown(Key);
    }
}

public class BindConfig(string name, KeyCode key, BindCategory category = BindCategory.Misc)
{
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
    public KeyCode Key { get; } = key;
    public BindCategory Category { get; } = category;
}

public enum BindCategory
{
    UniTAS,
    Movie,
    Misc
}