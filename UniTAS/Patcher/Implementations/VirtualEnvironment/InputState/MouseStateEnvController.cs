using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState;

[Singleton]
public class MouseStateEnvController : IMouseStateEnvController
{
    private readonly IMouseStateEnvLegacySystem _mouseStateEnvLegacySystem;
    private readonly IMouseStateEnvNewSystem _mouseStateEnvNewSystem;

    public MouseStateEnvController(IMouseStateEnvLegacySystem mouseStateEnvLegacySystem,
        IMouseStateEnvNewSystem mouseStateEnvNewSystem)
    {
        _mouseStateEnvLegacySystem = mouseStateEnvLegacySystem;
        _mouseStateEnvNewSystem = mouseStateEnvNewSystem;
    }

    public void SetPosition(Vector2 position)
    {
        _mouseStateEnvNewSystem.Position = position;
        _mouseStateEnvLegacySystem.Position = position;
    }

    public void SetPositionRelative(Vector2 position)
    {
        _mouseStateEnvNewSystem.Position += position;
        _mouseStateEnvLegacySystem.Position += position;
    }

    public void SetScroll(Vector2 scroll)
    {
        _mouseStateEnvNewSystem.Scroll = scroll;
        _mouseStateEnvLegacySystem.Scroll = scroll;
    }

    public void HoldButton(MouseButton button)
    {
        _mouseStateEnvNewSystem.HoldButton(button);
        _mouseStateEnvLegacySystem.HoldButton(button);
    }

    public void ReleaseButton(MouseButton button)
    {
        _mouseStateEnvNewSystem.ReleaseButton(button);
        _mouseStateEnvLegacySystem.ReleaseButton(button);
    }
}