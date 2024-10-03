using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState;

[Singleton]
public class MouseStateEnvController(
    IMouseStateEnvLegacySystem mouseStateEnvLegacySystem,
    IMouseStateEnvNewSystem mouseStateEnvNewSystem)
    : IMouseStateEnvController
{
    public void SetPosition(Vector2 position)
    {
        mouseStateEnvNewSystem.Position = position;
        mouseStateEnvLegacySystem.Position = position;
    }

    public void SetPositionRelative(Vector2 position)
    {
        mouseStateEnvNewSystem.Position += position;
        mouseStateEnvLegacySystem.Position += position;
    }

    public void SetScroll(Vector2 scroll)
    {
        mouseStateEnvNewSystem.Scroll = scroll;
        mouseStateEnvLegacySystem.Scroll = scroll;
    }

    public void HoldButton(MouseButton button)
    {
        mouseStateEnvNewSystem.HoldButton(button);
        mouseStateEnvLegacySystem.HoldButton(button);
    }

    public void ReleaseButton(MouseButton button)
    {
        mouseStateEnvNewSystem.ReleaseButton(button);
        mouseStateEnvLegacySystem.ReleaseButton(button);
    }
}