using UniTAS.Plugin.Models.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Plugin.Services.VirtualEnvironment.Input.NewInputSystem;

public interface IMouseStateEnvNewSystem
{
    Vector2 Position { get; set; }
    Vector2 Scroll { get; set; }
    void HoldButton(MouseButton button);
    void ReleaseButton(MouseButton button);
    bool IsButtonHeld(MouseButton button);
}