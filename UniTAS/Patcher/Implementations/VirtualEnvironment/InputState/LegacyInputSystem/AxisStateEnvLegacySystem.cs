using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.VirtualEnvironment;
using UniTAS.Patcher.Models.UnityInfo;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.UnityInfo;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState.LegacyInputSystem;

[Singleton]
public class AxisStateEnvLegacySystem : LegacyInputSystemDevice, IAxisStateEnvLegacySystem, ILegacyInputInfo
{
    private readonly Dictionary<string, LegacyInputAxisState> _values = new();

    private readonly List<LegacyInputAxis> _axisInfo = new();

    protected override void Update()
    {
    }

    protected override void FlushBufferedInputs()
    {
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
        _axisInfo.Add(axis);
    }

    public void KeyDown(string key)
    {
        foreach (var axisInfo in _axisInfo)
        {
            var negative = axisInfo.NegativeButton;
            var positive = axisInfo.PositiveButton;
            var negativeAlt = axisInfo.AltNegativeButton;
            var positiveAlt = axisInfo.AltPositiveButton;
        }
    }

    public void KeyUp(string key)
    {
    }
}