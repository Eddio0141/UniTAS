using System;
using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Plugin.Interfaces.Movie;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;
using UnityEngine;

namespace UniTAS.Plugin.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Key : EngineMethodClass
{
    private readonly IKeyboardStateEnv _keyboardStateEnv;

    [MoonSharpHidden]
    public Key(IKeyboardStateEnv keyboardStateEnv)
    {
        _keyboardStateEnv = keyboardStateEnv;
    }

    public void Hold(string key)
    {
        var parsedKey = ParseKeyCode(key);
        if (parsedKey.HasValue)
        {
            _keyboardStateEnv.Hold(new(parsedKey.Value));
            return;
        }

        _keyboardStateEnv.Hold(new(key));
    }

    public void Release(string key)
    {
        var parsedKey = ParseKeyCode(key);
        if (parsedKey.HasValue)
        {
            _keyboardStateEnv.Release(new(parsedKey.Value));
            return;
        }

        _keyboardStateEnv.Release(new(key));
    }

    public void Clear()
    {
        _keyboardStateEnv.Clear();
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