using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Models.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;
using UniTAS.Plugin.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UniTAS.Plugin.Services.VirtualEnvironment.Input.NewInputSystem;
using UnityEngine;

namespace UniTAS.Plugin.Implementations.VirtualEnvironment.InputState;

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