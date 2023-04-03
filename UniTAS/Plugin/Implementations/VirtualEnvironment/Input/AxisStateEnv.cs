using System.Collections.Generic;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;

namespace UniTAS.Plugin.Implementations.VirtualEnvironment.Input;

[Singleton]
public class AxisStateEnv : InputDevice, IAxisStateEnv
{
    public Dictionary<string, float> Values { get; }

    public AxisStateEnv()
    {
        Values = new();
    }

    protected override void Update()
    {
    }

    protected override void ResetState()
    {
        Values.Clear();
    }
}