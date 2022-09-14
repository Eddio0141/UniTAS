using System.Collections.Generic;

namespace UniTASPlugin.TAS.Input;

public static class Axis
{
    public static Dictionary<string, float> Values { get; internal set; }

    static Axis()
    {
        Values = new();
    }
}
