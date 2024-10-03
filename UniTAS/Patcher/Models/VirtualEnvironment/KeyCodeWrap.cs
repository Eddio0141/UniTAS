using System;
using UnityEngine;

namespace UniTAS.Patcher.Models.VirtualEnvironment;

public readonly struct KeyCodeWrap : IEquatable<KeyCodeWrap>
{
    public KeyCode KeyCode { get; } = KeyCode.None;

    public KeyCodeWrap(KeyCode keyCode)
    {
        KeyCode = keyCode;
    }

    public bool Equals(KeyCodeWrap other)
    {
        return KeyCode == other.KeyCode;
    }

    public override string ToString()
    {
        return KeyCode.ToString();
    }
}