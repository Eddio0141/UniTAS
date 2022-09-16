using Core.UnityHooks.InputLegacy;
using System;
using System.Collections.Generic;

namespace Core.TAS.Input.Movie;

public class Keys
{
    public List<KeyCodeType> Pressed;

    public Keys() : this(new()) { }

    public Keys(List<KeyCodeType> pressed)
    {
        Pressed = pressed ?? throw new ArgumentNullException(nameof(pressed));
    }
}
