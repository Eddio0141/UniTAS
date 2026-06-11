using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events;
using UniTAS.Patcher.Interfaces.Events.Movie;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Models.UnityInfo;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState;

[Singleton(RegisterPriority.AxisState)]
public class AxisState : IAxisState, IOnVirtualEnvStatusChange, IOnGameRestart, IOnMovieUpdate
{
    private readonly BufferedFullKeyState<string> _button = new();
    private readonly Dictionary<string, List<LegacyInputAxisState>> _values = [];

    public void MovieUpdate(bool fixedUpdate)
    {
        if (fixedUpdate) return;

        // this just needs to be flushed before legacy input system input is read so its fine to be pre-updates

        // TODO: add unit tests for this
        var pressedNames = new HashSet<string>();
        foreach (var pair in _values)
        {
            var name = pair.Key;
            foreach (var axis in pair.Value)
            {
                axis.FlushBufferedInputs();

                if (pressedNames.Contains(name)) continue;

                // as long as value isn't 0, it is pressed
                if (axis.ValueRaw != 0f)
                {
                    _button.Hold(name);
                    pressedNames.Add(name);
                }
                else
                {
                    _button.Release(name);
                }
            }
        }

        _button.Update();
    }

    private void ResetState()
    {
        foreach (var pair in _values)
        {
            foreach (var axis in pair.Value)
            {
                axis.ResetState();
            }
        }

        _button.ResetState();
    }

    public float GetAxis(string axisName)
    {
        if (!_values.TryGetValue(axisName, out var axisStates)) return 0;

        var value = 0f;
        foreach (var axisState in axisStates)
        {
            var axisStateValue = axisState.Value;
            // TODO: is this correct when the value is negative, i added Abs in case but idk
            if (Math.Abs(axisStateValue) > Math.Abs(value))
            {
                value = axisStateValue;
            }
        }

        return value;
    }

    public float GetAxisRaw(string axisName)
    {
        if (!_values.TryGetValue(axisName, out var axisStates)) return 0;

        var value = 0f;
        foreach (var axisState in axisStates)
        {
            var axisStateValue = axisState.ValueRaw;
            // TODO: is this correct when the value is negative, i added Abs in case but idk
            if (Math.Abs(axisStateValue) > Math.Abs(value))
            {
                value = axisStateValue;
            }
        }

        return value;
    }

    public void AddAxis(LegacyInputAxis axis)
    {
        var axisState = new LegacyInputAxisState(axis);
        if (_values.TryGetValue(axis.Name, out var value))
            value.Add(axisState);
        else
            _values.Add(axis.Name, [axisState]);
    }

    public void KeyDown(KeyCode key, JoyNum joystickNumber)
    {
        foreach (var pair in _values)
        {
            foreach (var axis in pair.Value)
            {
                if (axis.JoyNumEquals(joystickNumber))
                    axis.KeyDown(key);
            }
        }
    }

    public void KeyUp(KeyCode key, JoyNum joystickNumber)
    {
        foreach (var pair in _values)
        {
            foreach (var axis in pair.Value)
            {
                if (axis.JoyNumEquals(joystickNumber))
                    axis.KeyUp(key);
            }
        }
    }

    public void MouseMoveRel(Vector2 pos)
    {
        foreach (var pair in _values)
        {
            foreach (var axis in pair.Value)
            {
                axis.MousePos = pos;
            }
        }
    }

    public void SetAxis(AxisChoice axis, float value)
    {
        foreach (var pair in _values)
        {
            foreach (var axisState in pair.Value)
            {
                if (axisState.Axis.Axis == axis)
                    axisState.SetAxis(value);
            }
        }
    }

    public void MouseScroll(float scroll)
    {
        foreach (var pair in _values)
        {
            foreach (var axis in pair.Value)
            {
                if (axis.Axis.Type == AxisType.MouseMovement)
                    axis.SetAxis(scroll);
            }
        }
    }

    public void OnVirtualEnvStatusChange(bool runVirtualEnv)
    {
        if (!runVirtualEnv) return;

        ResetState();
    }

    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        ResetState();
    }

    public bool IsButtonHeld(string button)
    {
        return _button.IsHeld(button);
    }

    public bool IsButtonDown(string button)
    {
        return _button.IsDown(button);
    }

    public bool IsButtonUp(string button)
    {
        return _button.IsUp(button);
    }
}
