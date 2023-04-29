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
    private readonly IKeyboardStateEnvController _kbController;

    [MoonSharpHidden]
    public Key(IKeyboardStateEnvController kbController)
    {
        _kbController = kbController;
    }

    public void Hold(string key)
    {
        var parsedKey = ParseKeyCode(key);
        if (parsedKey.HasValue)
        {
            _kbController.Hold(new(parsedKey.Value));
            return;
        }

        _kbController.Hold(new(key));
    }

    public void Release(string key)
    {
        var parsedKey = ParseKeyCode(key);
        if (parsedKey.HasValue)
        {
            _kbController.Release(new(parsedKey.Value));
            return;
        }

        _kbController.Release(new(key));
    }

    public void Clear()
    {
        _kbController.Clear();
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