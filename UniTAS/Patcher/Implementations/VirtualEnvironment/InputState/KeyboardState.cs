using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events;
using UniTAS.Patcher.Interfaces.Events.Movie;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState;

[Singleton]
public class KeyboardStateEnvNewSystem : IOnVirtualEnvStatusChange, IOnGameRestart, IKeyboardStateNew, IKeyboardStateEnvLegacySystem, IKeyboardState, IOnMovieUpdate
{
    private readonly BufferedFullKeyState<KeyCodeWrap> _keyStateOld = new();
    IReadOnlyCollection<KeyCodeWrap> IKeyboardStateEnvLegacySystem.HeldKeys => _keyStateOld.Held;
    public HashSet<NewKeyCodeWrap> HeldKeys { get; } = [];

    public bool AnyKeyHeld => _keyStateOld.Held.Count > 0;

    public bool AnyKeyDown => _keyStateOld.Down.Count > 0;

    public void Hold(KeyCode? keyCode, Key? key)
    {
        if (keyCode != null)
        {
            _keyStateOld.Hold(new KeyCodeWrap(keyCode.Value));
        }
        if (key != null)
        {
            var keyCodeWrap = new NewKeyCodeWrap(key.Value);
            HeldKeys.Add(keyCodeWrap);
        }
    }

    public void Release(KeyCode? keyCode, Key? key)
    {
        if (keyCode != null)
        {
            _keyStateOld.Release(new KeyCodeWrap(keyCode.Value));
        }
        if (key != null)
        {
            var keyCodeWrap = new NewKeyCodeWrap(key.Value);
            HeldKeys.Remove(keyCodeWrap);
        }
    }

    public void Clear()
    {
        HeldKeys.Clear();
        _keyStateOld.Clear();
    }

    public void OnVirtualEnvStatusChange(bool runVirtualEnv)
    {
        if (!runVirtualEnv) return;
        Clear();
    }

    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        if (!preSceneLoad) return;
        Clear();
    }

    public bool IsKeyDown(KeyCode keyCode)
    {
        return _keyStateOld.IsDown(new KeyCodeWrap(keyCode));
    }

    public bool IsKeyUp(KeyCode keyCode)
    {
        return _keyStateOld.IsUp(new KeyCodeWrap(keyCode));
    }

    public bool IsKeyHeld(KeyCode keyCode)
    {
        return _keyStateOld.IsHeld(new KeyCodeWrap(keyCode));
    }

    public void MovieUpdate(bool fixedUpdate)
    {
        if (fixedUpdate) return;

        _keyStateOld.Update();
    }
}
