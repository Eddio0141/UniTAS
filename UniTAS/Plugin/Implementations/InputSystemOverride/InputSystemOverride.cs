using System;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.DontRunIfPaused;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Plugin.Services.Movie;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;

namespace UniTAS.Plugin.Implementations.InputSystemOverride;

[Singleton]
public class InputSystemOverride : IOnAwakeUnconditional, IOnPreUpdatesActual
{
    private readonly bool _hasInputSystem;

    private TASMouse _mouse;
    private TASKeyboard _keyboard;

    private readonly IMovieRunner _movieRunner;
    private readonly IMouseStateEnv _mouseStateEnv;
    private readonly IKeyboardStateEnv _keyboardStateEnv;

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    public InputSystemOverride(IMovieRunner movieRunner, IMouseStateEnv mouseStateEnv,
        IKeyboardStateEnv keyboardStateEnv)
    {
        _movieRunner = movieRunner;
        _mouseStateEnv = mouseStateEnv;
        _keyboardStateEnv = keyboardStateEnv;

        try
        {
            if (Mouse.current != null)
            {
                _hasInputSystem = true;
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    [InputControlLayout(stateType = typeof(MouseState), isGenericTypeOfDevice = true)]
    private class TASMouse : Mouse
    {
    }

    [InputControlLayout(stateType = typeof(KeyboardState), isGenericTypeOfDevice = true)]
    private class TASKeyboard : Keyboard
    {
    }

    public void AwakeUnconditional()
    {
        if (!_hasInputSystem) return;
        if (_mouse != null) return;

        _mouse = InputSystem.AddDevice<TASMouse>();
        _keyboard = InputSystem.AddDevice<TASKeyboard>();
    }

    public void PreUpdateActual()
    {
        if (!_hasInputSystem || _mouse == null || _movieRunner.MovieEnd) return;

        var state = new MouseState
        {
            // bit 0 = left mouse button
            buttons = (ushort)(_mouseStateEnv.LeftClick ? 1 : 0),
        };

        // var keys = new byte[14];
        // if (_keyboardStateEnv.Keys.Contains(KeyCode.Space)) keys[0] = 1;

        var stateKeyboard = new KeyboardState();
        unsafe
        {
            stateKeyboard.keys[0] = 1;
        }

        InputSystem.QueueStateEvent(_mouse, state);
    }
}