using System.Collections.Generic;
using System.Linq;
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
    private readonly List<(string, LegacyInputAxisState)> _values = [];

    protected override void Update()
    {
    }

    protected override void FlushBufferedInputs()
    {
        foreach (var (name, axis) in _values)
        {
            axis.FlushBufferedInputs();

            if (axis.ValueRaw != 0f)
            {
                axisButtonStateEnvUpdate.Hold(name);
            }
            else
            {
                axisButtonStateEnvUpdate.Release(name);
            }
        }
    }

    protected override void ResetState()
    {
        foreach (var (_, value) in _values)
        {
            value.ResetState();
        }
    }

    public float GetAxis(string axisName)
    {
        var value = 0f;
        var found = _values.Where(x => x.Item1 == axisName);

        foreach (var (_, axisState) in found)
        {
            var axisStateValue = axisState.Value;
            if (axisStateValue > value)
            {
                value = axisStateValue;
            }
        }

        return value;
    }

    public float GetAxisRaw(string axisName)
    {
        var value = 0f;
        var found = _values.Where(x => x.Item1 == axisName);
        foreach (var (_, axisState) in found)
        {
            var axisStateValue = axisState.ValueRaw;
            if (axisStateValue > value)
            {
                value = axisStateValue;
            }
        }

        return value;
    }

    public void AddAxis(LegacyInputAxis axis)
    {
        var axisState = new LegacyInputAxisState(axis);
        _values.Add((axis.Name, axisState));
    }

    public void KeyDown(string key, JoyNum joystickNumber)
    {
        foreach (var (_, axis) in _values)
        {
            if (!axis.JoyNumEquals(joystickNumber)) continue;

            axis.KeyDown(key);
        }
    }

    public void KeyUp(string key, JoyNum joystickNumber)
    {
        foreach (var (_, axis) in _values)
        {
            if (!axis.JoyNumEquals(joystickNumber)) continue;

            axis.KeyUp(key);
        }
    }

    public void MouseMove(Vector2 pos)
    {
        foreach (var (_, axis) in _values)
        {
            axis.MousePos = pos;
        }
    }

    public void SetAxis(AxisChoice axis, float value)
    {
        foreach (var (_, axisState) in _values)
        {
            if (axisState.Axis.Axis == axis)
            {
                axisState.SetAxis(value);
            }
        }
    }

    public void MouseMoveRelative(Vector2 pos)
    {
        foreach (var (_, axis) in _values)
        {
            axis.MouseMoveRelative(pos);
        }
    }

    public void MouseScroll(float scroll)
    {
        foreach (var (_, axis) in _values)
        {
            if (axis.Axis.Type != AxisType.MouseMovement) return;
            axis.SetAxis(scroll);
        }
    }
}