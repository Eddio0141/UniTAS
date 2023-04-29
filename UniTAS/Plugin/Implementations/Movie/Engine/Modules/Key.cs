using System;
using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Plugin.Interfaces.Movie;
using UniTAS.Plugin.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UnityEngine;

namespace UniTAS.Plugin.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Key : EngineMethodClass
{
    private readonly IKeyboardStateEnvLegacySystem _keyboardStateEnvLegacySystem;

    [MoonSharpHidden]
    public Key(IKeyboardStateEnvLegacySystem keyboardStateEnvLegacySystem)
    {
        _keyboardStateEnvLegacySystem = keyboardStateEnvLegacySystem;
    }

    public void Hold(string key)
    {
        var parsedKey = ParseKeyCode(key);
        if (parsedKey.HasValue)
        {
            _keyboardStateEnvLegacySystem.Hold(new(parsedKey.Value));
            return;
        }

        _keyboardStateEnvLegacySystem.Hold(new(key));
    }

    public void Release(string key)
    {
        var parsedKey = ParseKeyCode(key);
        if (parsedKey.HasValue)
        {
            _keyboardStateEnvLegacySystem.Release(new(parsedKey.Value));
            return;
        }

        _keyboardStateEnvLegacySystem.Release(new(key));
    }

    public void Clear()
    {
        _keyboardStateEnvLegacySystem.Clear();
    }

    private static KeyCode? ParseKeyCode(string key)
    {
        if (Enum.IsDefined(typeof(KeyCode), key))
        {
            return (KeyCode)Enum.Parse(typeof(KeyCode), key, true);
        }

        return null;
    }
}