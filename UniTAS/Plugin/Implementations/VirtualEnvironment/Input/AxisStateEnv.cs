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

    public override void Update()
    {
    }

    public override void ResetState()
    {
        Values.Clear();
    }
}