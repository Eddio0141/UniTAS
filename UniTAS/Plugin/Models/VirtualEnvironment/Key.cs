using System;
using UnityEngine;

namespace UniTAS.Plugin.Models.VirtualEnvironment;

public readonly struct Key : IEquatable<Key>
{
    private string Keys { get; } = null;
    private KeyCode? KeyCode { get; } = null;
    public UnityEngine.InputSystem.Key? NewInputSystemKey { get; }

    public Key(string keys, UnityEngine.InputSystem.Key? newInputSystemKey)
    {
        Keys = keys;
        NewInputSystemKey = newInputSystemKey;
    }

    public Key(KeyCode keyCode, UnityEngine.InputSystem.Key? newInputSystemKey)
    {
        KeyCode = keyCode;
        NewInputSystemKey = newInputSystemKey;
    }

    public bool Equals(Key other)
    {
        return Keys == other.Keys && KeyCode == other.KeyCode && NewInputSystemKey == other.NewInputSystemKey;
    }

    public override bool Equals(object obj)
    {
        return obj is Key other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (Keys != null ? Keys.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ KeyCode.GetHashCode();
            hashCode = (hashCode * 397) ^ NewInputSystemKey.GetHashCode();
            return hashCode;
        }
    }

    public override string ToString()
    {
        return Keys ?? KeyCode.ToString();
    }
}