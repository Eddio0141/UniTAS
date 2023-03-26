using System.Collections.Generic;

namespace UniTAS.Plugin.Services.VirtualEnvironment.Input;

public interface IAxisStateEnv
{
    public Dictionary<string, float> Values { get; }
}