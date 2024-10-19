using System.Collections.ObjectModel;
using UniTAS.Patcher.Models.UnityInfo;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;

public interface IAxisStateEnvLegacySystem
{
    float GetAxis(string axisName);
    float GetAxisRaw(string axisName);
    void SetAxis(AxisChoice axis, float value);
    void KeyDown(KeyCode key, JoyNum joystickNumber);
    void KeyUp(KeyCode key, JoyNum joystickNumber);
    void MouseMove(Vector2 pos);
    void MouseMoveRelative(Vector2 pos);
    void MouseScroll(float scroll);
    void AddAxis(LegacyInputAxis axis);
    ReadOnlyCollection<(string, LegacyInputAxisState)> AllAxis { get; }
}