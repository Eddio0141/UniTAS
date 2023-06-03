namespace UniTAS.Plugin.Services.VirtualEnvironment.Input.LegacyInputSystem;

public interface IAxisStateEnvLegacySystem
{
    float GetAxis(string axisName);
    void SetAxis(string axisName, float value);
}