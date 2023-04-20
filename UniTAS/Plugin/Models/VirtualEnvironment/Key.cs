using UnityEngine;

namespace UniTAS.Plugin.Models.VirtualEnvironment;

public class Key
{
    public string Keys { get; }
    public KeyCode? KeyCode { get; }

    public Key(string keys)
    {
        Keys = keys;
    }

    public Key(KeyCode keyCode)
    {
        KeyCode = keyCode;
    }
}