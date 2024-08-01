using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState.NewInputSystem;

[Singleton]
public class MouseStateEnvNewSystem : Interfaces.VirtualEnvironment.InputState, IMouseStateEnvNewSystem
{
    public Vector2 Position { get; set; }
    public Vector2 Delta { get; private set; }
    public Vector2 Scroll { get; set; }
    private Vector2 _prevPos;
    private readonly List<MouseButton> _heldButtons = new();

    protected override void ResetState()
    {
        Position = Vector2.zero;
        _prevPos = Vector2.zero;
        Delta = Vector2.zero;
        Scroll = Vector2.zero;
    }

    protected override void Update()
    {
        base.Update();

        Delta = Position - _prevPos;
        _prevPos = Position;
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
