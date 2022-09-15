using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniTASPlugin.TAS.Input.Movie;

public class Keys
{
    public List<KeyCode> Pressed;

    public Keys() : this(new()) { }

    public Keys(List<KeyCode> pressed)
    {
        Pressed = pressed ?? throw new ArgumentNullException(nameof(pressed));
    }
}
