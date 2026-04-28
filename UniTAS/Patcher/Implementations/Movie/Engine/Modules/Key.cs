using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.Movie;
using UniTAS.Patcher.Models.UnityInfo;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.Movie;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[method: MoonSharpHidden]
public class Key(
    IKeyboardState keyboard,
    IAxisState axisState,
    IMovieRunner movieRunner,
    IInputSystemState inputSystemState)
    : EngineMethodClass
{
    public void Hold(string key)
    {
        var (keyCode, newKey) = ParseKeyCodeAndWarn(key);

        keyboard.Hold(keyCode, newKey);
        if (keyCode.HasValue)
            axisState.KeyDown(keyCode.Value, JoyNum.AllJoysticks);
    }

    public void Release(string key)
    {
        var (keyCode, newKey) = ParseKeyCodeAndWarn(key);

        keyboard.Release(keyCode, newKey);
        if (keyCode.HasValue)
            axisState.KeyUp(keyCode.Value, JoyNum.AllJoysticks);
    }

    public void Clear()
    {
        keyboard.Clear();
    }

    private (KeyCode?, UnityEngine.InputSystem.Key?) ParseKeyCodeAndWarn(string key)
    {
        InputSystemUtils.KeyStringToKeys(key, out var keyCode, out var newKey);

        if (!keyCode.HasValue && inputSystemState.HasOldInputSystem)
        {
            movieRunner.MovieLogger.LogWarning(
                "failed to find matching KeyCode, make sure it's a valid entry in UnityEngine.KeyCode");
        }

        if (!newKey.HasValue && inputSystemState.HasNewInputSystem)
        {
            movieRunner.MovieLogger.LogWarning("failed to find matching keycode for new unity input system");
        }

        return (keyCode, newKey);
    }
}
