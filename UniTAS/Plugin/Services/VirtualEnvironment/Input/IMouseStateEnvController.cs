using UniTAS.Plugin.Models.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Plugin.Services.VirtualEnvironment.Input;

/// <summary>
/// Sets all mouse states.
/// </summary>
public interface IMouseStateEnvController
{
    void SetPosition(Vector2 position);
    void SetPositionRelative(Vector2 position);
    void SetScroll(Vector2 scroll);
    void HoldButton(MouseButton button);
    void ReleaseButton(MouseButton button);
}