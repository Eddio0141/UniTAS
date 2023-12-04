using System.Collections.Generic;
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
    private readonly Dictionary<string, LegacyInputAxisState> _values = new();

    protected override void Update()
    {
    }

    protected override void FlushBufferedInputs()
    {
        foreach (var value in _values)
        {
            var axis = value.Value;
            axis.FlushBufferedInputs();
        }
    }

    protected override void ResetState()
    {
        _values.Clear();
    }

    public float GetAxis(string axisName)
    {
        return _values.TryGetValue(axisName, out var value) ? value.Value : 0f;
    }

    public float GetAxisRaw(string axisName)
    {
        return _values.TryGetValue(axisName, out var value) ? value.ValueRaw : 0f;
    }

    public void AddAxis(LegacyInputAxis axis)
    {
        var axisState = new LegacyInputAxisState(axis);
        _values.Add(axis.Name, axisState);
    }

    public void KeyDown(string key)
    {
        foreach (var value in _values)
        {
            var axis = value.Value;
            axis.KeyDown(key);
        }
    }

    public void KeyUp(string key)
    {
        foreach (var value in _values)
        {
            var axis = value.Value;
            axis.KeyUp(key);
        }
    }

    public void MouseMove(Vector2 pos)
    {
        foreach (var value in _values)
        {
            var axis = value.Value;
            axis.MousePos = pos;
        }
    }

    public void SetAxis(AxisChoice axis, float value)
    {
        foreach (var axisState in _values)
        {
            var axisStateValue = axisState.Value;
            if (axisStateValue.Axis.Axis == axis)
            {
                axisState.Value.SetAxis(value);
            }
        }
    }

    public void MouseMoveRelative(Vector2 pos)
    {
        foreach (var value in _values)
        {
            var axis = value.Value;
            axis.MouseMoveRelative(pos);
        }
    }

    public void MouseScroll(float scroll)
    {
        foreach (var value in _values)
        {
            var axis = value.Value;
            if (axis.Axis.Type != AxisType.MouseMovement) return;
            axis.SetAxis(scroll);
        }
    }
}