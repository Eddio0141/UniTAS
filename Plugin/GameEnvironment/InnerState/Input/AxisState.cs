using System.Collections.Generic;

namespace UniTASPlugin.GameEnvironment.InnerState.Input;

public class AxisState : InputDeviceBase
{
    public Dictionary<string, float> Values { get; set; }

    public AxisState()
    {
        Values = new Dictionary<string, float>();
    }

    public override void Update()
    {
    }

    public override void ResetState()
    {
        Values.Clear();
    }
}