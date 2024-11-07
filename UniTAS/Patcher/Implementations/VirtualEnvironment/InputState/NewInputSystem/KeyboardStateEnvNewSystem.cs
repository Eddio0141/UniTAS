using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState.NewInputSystem;

[Singleton]
public class KeyboardStateEnvNewSystem : Interfaces.VirtualEnvironment.InputState, IKeyboardStateEnvNewSystem
{
    public HashSet<NewKeyCodeWrap> HeldKeys { get; } = new();

    public void Hold(Key key)
    {
        var keyCodeWrap = new NewKeyCodeWrap(key);
        HeldKeys.Add(keyCodeWrap);
    }

    public void Release(Key key)
    {
        var keyCodeWrap = new NewKeyCodeWrap(key);
        HeldKeys.Remove(keyCodeWrap);
    }

    public void Clear()
    {
        HeldKeys.Clear();
    }

    protected override void ResetState()
    {
        HeldKeys.Clear();
    }
}