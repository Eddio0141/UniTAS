using System.Collections.Generic;

namespace UniTASPlugin.GameEnvironment.InnerState.Input;

public class AxisState
{
    public Dictionary<string, float> Values { get; set; }

    public AxisState()
    {
        Values = new Dictionary<string, float>();
    }
}