using UniTAS.Plugin.Models.VirtualEnvironment;

namespace UniTAS.Plugin.Services.VirtualEnvironment.Input;

/// <summary>
/// Sets all keyboard states.
/// </summary>
public interface IKeyboardStateEnvController
{
    void Hold(Key key);
    void Release(Key key);
    void Clear();
}