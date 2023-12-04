using UniTAS.Patcher.Models.UnityInfo;
using UnityEngine;

namespace UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;

public interface IAxisStateEnvLegacySystem
{
    float GetAxis(string axisName);
    float GetAxisRaw(string axisName);
    void SetAxis(AxisChoice axis, float value);
    void KeyDown(string key);
    void KeyUp(string key);
    void MouseMove(Vector2 pos);
    void MouseMoveRelative(Vector2 pos);
    void MouseScroll(float scroll);
    void AddAxis(LegacyInputAxis axis);
}