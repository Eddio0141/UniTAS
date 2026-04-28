using UnityEngine;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Services.VirtualEnvironment.Input;

public interface IKeyboardState
{
    void Hold(KeyCode? keyCode, Key? key);
    void Release(KeyCode? keyCode, Key? key);
    void Clear();
}

