using UniTAS.Patcher.Models.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;

public interface IMouseStateEnvNewSystem
{
    Vector2 Position { get; set; }
    Vector2 Scroll { get; set; }
    void HoldButton(MouseButton button);
    void ReleaseButton(MouseButton button);
    bool IsButtonHeld(MouseButton button);
}