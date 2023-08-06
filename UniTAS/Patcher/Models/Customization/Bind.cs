using System;
using System.Diagnostics.CodeAnalysis;
using BepInEx;
using BepInEx.Configuration;
using UniTAS.Patcher.Services;
using UnityEngine;

namespace UniTAS.Patcher.Models.Customization;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class Bind : IEquatable<Bind>
{
    private KeyCode Key
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

    private readonly IPatchReverseInvoker _patchReverseInvoker;
    private readonly IConfig _config;
    private KeyCode _key;

    private const string CONFIG_SECTION = "Binds";

    public Bind(BindConfig bindConfig, IPatchReverseInvoker patchReverseInvoker, IConfig config)
    {
        if (bindConfig == null) throw new ArgumentNullException(nameof(bindConfig));
        _key = bindConfig.Key;
        _patchReverseInvoker = patchReverseInvoker;
        _config = config;
        Name = bindConfig.Name;
    }

    private bool _initialized;

    public void InitConfig()
    {
        if (_initialized) return;
        _initialized = true;
        _keyConfigEntry = _config.ConfigFile.Bind(CONFIG_SECTION, Name, _key.ToString(),
            "Key to press to trigger this bind. See https://docs.unity3d.com/ScriptReference/KeyCode.html for a list of keys.");
    }

    public bool IsPressed()
    {
        return _patchReverseInvoker.Invoke(key => UnityInput.Current.GetKeyDown(key), Key);
    }

    public bool Equals(Bind other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Bind)obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}

public class BindConfig
{
    public string Name { get; }
    public KeyCode Key { get; }

    public BindConfig(string name, KeyCode key)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Key = key;
    }
}