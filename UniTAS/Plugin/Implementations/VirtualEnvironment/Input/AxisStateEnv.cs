using System.Collections.Generic;
using UniTAS.Plugin.Interfaces.DependencyInjection;

namespace UniTAS.Plugin.Services.VirtualEnvironment.InnerState.Input;

[Singleton]
public class AxisStateEnv : InputDevice, IAxisStateEnv
{
    public Dictionary<string, float> Values { get; }

    public AxisStateEnv()
    {
        Values = new();
    }

    public override void Update()
    {
    }

    public override void ResetState()
    {
        Values.Clear();
    }
}