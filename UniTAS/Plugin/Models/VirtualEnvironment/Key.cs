using UnityEngine;

namespace UniTAS.Plugin.Models.VirtualEnvironment;

public class Key
{
    public string keys { get; }
    public KeyCode? KeyCode { get; }

    public Key(string keys)
    {
        this.keys = keys;
    }

    public Key(KeyCode keyCode)
    {
        KeyCode = keyCode;
    }
}