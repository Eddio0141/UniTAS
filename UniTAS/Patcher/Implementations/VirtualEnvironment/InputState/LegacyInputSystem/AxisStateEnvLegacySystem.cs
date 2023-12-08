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
public class AxisStateEnvLegacySystem : LegacyInputSystemDevice, IAxisStateEnvLegacySystem
{
    private readonly IAxisButtonStateEnvUpdate _axisButtonStateEnvUpdate;

    private readonly List<(string, LegacyInputAxisState)> _values = new();

    public AxisStateEnvLegacySystem(IAxisButtonStateEnvUpdate axisButtonStateEnvUpdate)
    {
        _axisButtonStateEnvUpdate = axisButtonStateEnvUpdate;
    }

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
                _axisButtonStateEnvUpdate.Hold(name);
            }
            else
            {
                _axisButtonStateEnvUpdate.Release(name);
            }
        }
    }

    protected override void ResetState()
    {
        _values.Clear();
    }

    public float GetAxis(string axisName)
    {
        var (_, found) = _values.FirstOrDefault(x => x.Item1 == axisName);
        return found?.Value ?? 0f;
    }

    public float GetAxisRaw(string axisName)
    {
        var (_, found) = _values.FirstOrDefault(x => x.Item1 == axisName);
        return found?.ValueRaw ?? 0f;
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