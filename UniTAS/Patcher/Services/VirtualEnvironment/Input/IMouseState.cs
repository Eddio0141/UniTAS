using UniTAS.Patcher.Models.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Patcher.Services.VirtualEnvironment.Input;

public interface IMouseState
{
    Vector2 Position { get; set; }
    Vector2 Scroll { get; set; }
    Vector2 Delta { get; }
    void HoldButton(MouseButton button);
    void ReleaseButton(MouseButton button);
    bool IsButtonHeld(MouseButton button);
}
