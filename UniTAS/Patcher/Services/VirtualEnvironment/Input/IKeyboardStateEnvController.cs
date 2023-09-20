namespace UniTAS.Patcher.Services.VirtualEnvironment.Input;

/// <summary>
/// Sets all keyboard states.
/// </summary>
public interface IKeyboardStateEnvController
{
    void Hold(string key);
    void Release(string key);
    void Clear();
}