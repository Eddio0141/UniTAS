using UnityEngine;

namespace UniTAS.Plugin.Models.VirtualEnvironment;

public class Key
{
    public char? Character { get; }
    public KeyCode? KeyCode { get; }

    public Key(char character)
    {
        Character = character;
    }

    public Key(KeyCode keyCode)
    {
        KeyCode = keyCode;
    }
}