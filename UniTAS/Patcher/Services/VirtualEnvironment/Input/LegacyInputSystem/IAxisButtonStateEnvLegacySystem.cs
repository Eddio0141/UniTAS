namespace UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;

public interface IAxisButtonStateEnvLegacySystem
{
    bool IsButtonHeld(string button);
    bool IsButtonDown(string button);
    bool IsButtonUp(string button);
}