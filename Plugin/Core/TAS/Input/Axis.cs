﻿using System.Collections.Generic;

namespace Core.TAS.Input;

public static class Axis
{
    public static Dictionary<string, float> Values { get; internal set; }

    static Axis()
    {
        Values = new();
    }

    public static void Clear()
    {
        Values.Clear();
    }
}
