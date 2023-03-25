using System.Collections.Generic;

namespace UniTAS.Plugin.Services.VirtualEnvironment.InnerState.Input;

public interface IAxisStateEnv
{
    public Dictionary<string, float> Values { get; }
}