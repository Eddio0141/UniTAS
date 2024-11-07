using System.Collections.Generic;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;

public interface IKeyboardStateEnvNewSystem
{
    void Hold(Key keyCodeWrap);
    void Release(Key keyCodeWrap);
    HashSet<NewKeyCodeWrap> HeldKeys { get; }
    void Clear();
}