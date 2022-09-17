using Core.UnityHooks.InputLegacy;
using System;
using System.Collections.Generic;

namespace Core.TAS.Input.Movie;

public class Keys
{
    public List<KeyCodeType_2021_2_14> Pressed;

    public Keys() : this(new()) { }

    public Keys(List<KeyCodeType_2021_2_14> pressed)
    {
        Pressed = pressed ?? throw new ArgumentNullException(nameof(pressed));
    }
}
