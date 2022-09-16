using Core.UnityHook.Types.InputLegacy;
using System;
using System.Collections.Generic;

namespace Core.TAS.Input.Movie;

public class Keys
{
    public List<KeyCodeTypes> Pressed;

    public Keys() : this(new()) { }

    public Keys(List<KeyCodeTypes> pressed)
    {
        Pressed = pressed ?? throw new ArgumentNullException(nameof(pressed));
    }
}
