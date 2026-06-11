using UniTAS.Patcher.Models.UnityInfo;
using UnityEngine;

namespace UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;

public interface IAxisState
{
    float GetAxis(string axisName);
    float GetAxisRaw(string axisName);
    void SetAxis(AxisChoice axis, float value);
    void KeyDown(KeyCode key, JoyNum joystickNumber);
    void KeyUp(KeyCode key, JoyNum joystickNumber);
    void MouseMoveRel(Vector2 pos);
    void MouseScroll(float scroll);
    void AddAxis(LegacyInputAxis axis);
    bool IsButtonHeld(string button);
    bool IsButtonDown(string button);
    bool IsButtonUp(string button);
}
