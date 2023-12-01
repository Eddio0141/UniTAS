using UniTAS.Patcher.Models.UnityInfo;

namespace UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;

public interface IAxisStateEnvLegacySystem
{
    float GetAxis(string axisName);
    float GetAxisRaw(string axisName);
    void KeyDown(string key);
    void KeyUp(string key);
    void AddAxis(LegacyInputAxis axis);
}