namespace UniTAS.Patcher.Services.VirtualEnvironment.Input;

/// <summary>
/// Sets all keyboard states.
/// </summary>
public interface IKeyboardStateEnvController
{
    void Hold(string key, out string warningMsg);
    void Release(string key, out string warningMsg);
    void Clear();
}