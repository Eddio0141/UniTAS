using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.VirtualEnvironment;
using UniTAS.Plugin.Models.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;
using UnityEngine;

namespace UniTAS.Plugin.Implementations.VirtualEnvironment.InputState;

[Singleton]
[ExcludeRegisterIfTesting]
public class MouseStateEnv : LegacyInputSystemButtonBasedDevice<MouseButtonWrap>, IMouseStateEnv
{
    public bool MousePresent => true;
    public Vector2 Position { get; set; }
    public Vector2 Scroll { get; set; }

    protected override void ResetState()
    {
        base.ResetState();
        Position = Vector2.zero;
        Scroll = Vector2.zero;
    }

    public bool IsButtonHeld(MouseButton button)
    {
        return IsButtonHeld(new MouseButtonWrap(button));
    }

    public bool IsButtonDown(MouseButton button)
    {
        return IsButtonDown(new MouseButtonWrap(button));
    }

    public bool IsButtonUp(MouseButton button)
    {
        return IsButtonUp(new MouseButtonWrap(button));
    }

    public void HoldButton(MouseButton button)
    {
        Hold(new(button));
    }

    public void ReleaseButton(MouseButton button)
    {
        Release(new(button));
    }

    public new bool AnyButtonHeld => base.AnyButtonHeld;
    public new bool AnyButtonDown => base.AnyButtonDown;
}