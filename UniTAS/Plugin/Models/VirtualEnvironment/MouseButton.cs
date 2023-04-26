using System;

namespace UniTAS.Plugin.Models.VirtualEnvironment;

public enum MouseButton
{
    Left,
    Right,
    Middle
}

public readonly struct MouseButtonWrap : IEquatable<MouseButtonWrap>
{
    private MouseButton MouseButton { get; }

    public MouseButtonWrap(MouseButton mouseButton)
    {
        MouseButton = mouseButton;
    }

    public bool Equals(MouseButtonWrap other)
    {
        return MouseButton == other.MouseButton;
    }

    public override bool Equals(object obj)
    {
        return obj is MouseButtonWrap other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (int)MouseButton;
    }
}