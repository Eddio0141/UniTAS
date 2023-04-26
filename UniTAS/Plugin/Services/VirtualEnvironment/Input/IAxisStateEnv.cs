namespace UniTAS.Plugin.Services.VirtualEnvironment.Input;

public interface IAxisStateEnv
{
    float GetAxis(string axisName);
    void SetAxis(string axisName, float value);
}