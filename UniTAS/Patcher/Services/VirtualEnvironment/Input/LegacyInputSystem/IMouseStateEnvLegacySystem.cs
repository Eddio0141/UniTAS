using UniTAS.Patcher.Models.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;

public interface IMouseStateEnvLegacySystem
{
    bool MousePresent { get; }
    Vector2 Position { get; set; }
    Vector2 Scroll { get; set; }
    bool IsButtonHeld(MouseButton button);
    bool IsButtonDown(MouseButton button);
    bool IsButtonUp(MouseButton button);
    void HoldButton(MouseButton button);
    void ReleaseButton(MouseButton button);
    bool AnyButtonHeld { get; }
    bool AnyButtonDown { get; }
}