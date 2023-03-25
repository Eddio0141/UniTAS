using System;
using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Plugin.Interfaces.Movie;
using UniTAS.Plugin.Services.VirtualEnvironment.InnerState.Input;
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
        _keyboardStateEnv.Hold(ParseKeyCode(key));
    }

    public void Release(string key)
    {
        _keyboardStateEnv.Release(ParseKeyCode(key));
    }

    public void Clear()
    {
        _keyboardStateEnv.Clear();
    }

    private static KeyCode ParseKeyCode(string key)
    {
        return (KeyCode)Enum.Parse(typeof(KeyCode), key, true);
    }
}