
namespace UniTAS.Plugin.Services.VirtualEnvironment.Input;

public interface IButtonStateEnv
{
    bool IsButtonHeld(string button);
    bool IsButtonDown(string button);
    bool IsButtonUp(string button);
    void Hold(string button);
    void Release(string button);
    void Clear();
}