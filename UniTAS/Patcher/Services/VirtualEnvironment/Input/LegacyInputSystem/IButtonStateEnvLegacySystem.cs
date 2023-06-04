namespace UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;

public interface IButtonStateEnvLegacySystem
{
    bool IsButtonHeld(string button);
    bool IsButtonDown(string button);
    bool IsButtonUp(string button);
    void Hold(string button);
    void Release(string button);
    void Clear();
}