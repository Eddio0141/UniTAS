using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.VirtualEnvironment;
using UniTAS.Patcher.Models.UnityInfo;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState.LegacyInputSystem;

[Singleton]
public class AxisStateEnvLegacySystem(IAxisButtonStateEnvUpdate axisButtonStateEnvUpdate)
    : LegacyInputSystemDevice, IAxisStateEnvLegacySystem
{
    private readonly Dictionary<string, List<LegacyInputAxisState>> _values = new();

    protected override void Update()
    {
    }

    protected override void FlushBufferedInputs()
    {
        // TODO: add unit tests for this
        var pressedNames = new HashSet<string>();
        foreach (var pair in _values)
        {
            var name = pair.Key;
            var axisStates = pair.Value;

            foreach (var axis in axisStates)
            {
                axis.FlushBufferedInputs();

                if (pressedNames.Contains(name)) continue;

                // as long as value isn't 0, it is pressed
                if (axis.ValueRaw != 0f)
                {
                    axisButtonStateEnvUpdate.Hold(name);
                    pressedNames.Add(name);
                }
                else
                {
                    axisButtonStateEnvUpdate.Release(name);
                }
            }
        }
    }

    protected override void ResetState()
    {
        foreach (var pair in _values)
        {
            foreach (var axis in pair.Value)
            {
                axis.ResetState();
            }
        }

        axisButtonStateEnvUpdate.ResetState();
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

    public void MouseMove(Vector2 pos)
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
}