using System;

namespace UniTAS.Patcher.Models.VirtualEnvironment;

public enum MouseButton
{
    Left,
    Right,
    Middle
}

public readonly struct MouseButtonWrap(MouseButton mouseButton) : IEquatable<MouseButtonWrap>
{
    private MouseButton MouseButton { get; } = mouseButton;

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

    public override string ToString()
    {
        return MouseButton.ToString();
    }
}
