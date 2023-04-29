using System.Collections.Generic;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Models.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input.NewInputSystem;
using UnityEngine;

namespace UniTAS.Plugin.Implementations.VirtualEnvironment.InputState.NewInputSystem;

[Singleton]
public class MouseStateEnvNewSystem : Interfaces.VirtualEnvironment.InputState, IMouseStateEnvNewSystem
{
    public Vector2 Position { get; set; }
    public Vector2 Scroll { get; set; }
    private readonly List<MouseButton> _heldButtons = new();

    protected override void ResetState()
    {
        Position = Vector2.zero;
        Scroll = Vector2.zero;
    }

    public void HoldButton(MouseButton button)
    {
        if (_heldButtons.Contains(button)) return;
        _heldButtons.Add(button);
    }

    public void ReleaseButton(MouseButton button)
    {
        _heldButtons.Remove(button);
    }

    public bool IsButtonHeld(MouseButton button)
    {
        return _heldButtons.Contains(button);
    }
}