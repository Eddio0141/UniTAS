using System;
using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Plugin.Interfaces.Movie;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Plugin.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Key : EngineMethodClass
{
    private readonly VirtualEnvironment _virtualEnvironment;

    [MoonSharpHidden]
    public Key(VirtualEnvironment virtualEnvironment)
    {
        _virtualEnvironment = virtualEnvironment;
    }

    public void Hold(string key)
    {
        _virtualEnvironment.InputState.KeyboardState.Hold(ParseKeyCode(key));
    }

    public void Release(string key)
    {
        _virtualEnvironment.InputState.KeyboardState.Release(ParseKeyCode(key));
    }

    public void Clear()
    {
        _virtualEnvironment.InputState.KeyboardState.Clear();
    }

    private static KeyCode ParseKeyCode(string key)
    {
        return (KeyCode)Enum.Parse(typeof(KeyCode), key, true);
    }
}