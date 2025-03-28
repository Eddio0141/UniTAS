using System.Collections.Generic;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;

public interface IKeyboardStateEnvLegacySystem
{
    HashSet<KeyCodeWrap> HeldKeys { get; }
    void Hold(KeyCode keyCode);
    void Release(KeyCode keyCode);
    void Clear();
    bool IsKeyDown(KeyCode keyCode);
    bool IsKeyUp(KeyCode keyCode);
    bool IsKeyHeld(KeyCode keyCode);
    bool AnyKeyHeld { get; }
    bool AnyKeyDown { get; }
}