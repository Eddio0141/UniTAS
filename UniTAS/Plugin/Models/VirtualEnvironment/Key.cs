using System;
using UnityEngine;

namespace UniTAS.Plugin.Models.VirtualEnvironment;

public readonly struct Key : IEquatable<Key>
{
    private string Keys { get; }
    private KeyCode? KeyCode { get; }

    public Key(string keys)
    {
        Keys = keys;
    }

    public Key(KeyCode keyCode)
    {
        KeyCode = keyCode;
    }

    public bool Equals(Key other)
    {
        // if (ReferenceEquals(null, other)) return false;
        // if (ReferenceEquals(this, other)) return true;
        return Keys == other.Keys && KeyCode == other.KeyCode;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        // if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Key)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((Keys != null ? Keys.GetHashCode() : 0) * 397) ^ KeyCode.GetHashCode();
        }
    }
}