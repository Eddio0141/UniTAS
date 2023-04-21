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

    private bool Equals(Key other)
    {
        return Keys == other.Keys && KeyCode == other.KeyCode;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
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