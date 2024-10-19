using UnityEngine;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Services.VirtualEnvironment.Input;

/// <summary>
/// Sets all keyboard states.
/// </summary>
public interface IKeyboardStateEnvController
{
    void Hold(KeyCode? keyCode, Key? key);
    void Release(KeyCode? keyCode, Key? key);
    void Clear();
}