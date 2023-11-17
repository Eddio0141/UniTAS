using System;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Models.VirtualEnvironment;

public readonly struct NewKeyCodeWrap : IEquatable<NewKeyCodeWrap>
{
    public Key Key { get; }

    public NewKeyCodeWrap(Key key)
    {
        Key = key;
    }

    public bool Equals(NewKeyCodeWrap other)
    {
        return Key == other.Key;
    }

    public override string ToString()
    {
        return Key.ToString();
    }
}