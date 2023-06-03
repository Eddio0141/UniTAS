using System.Collections.Generic;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Services.VirtualEnvironment.Input.LegacyInputSystem;

namespace UniTAS.Plugin.Implementations.VirtualEnvironment.InputState.LegacyInputSystem;

[Singleton]
public class AxisStateEnvLegacySystem : Interfaces.VirtualEnvironment.InputState, IAxisStateEnvLegacySystem
{
    private readonly Dictionary<string, float> _values = new();

    protected override void ResetState()
    {
        _values.Clear();
    }

    public float GetAxis(string axisName)
    {
        return _values.TryGetValue(axisName, out var value) ? value : 0f;
    }

    public void SetAxis(string axisName, float value)
    {
        _values[axisName] = value;
    }
}