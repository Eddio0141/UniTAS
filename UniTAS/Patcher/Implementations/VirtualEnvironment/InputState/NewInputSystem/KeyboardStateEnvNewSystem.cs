using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState.NewInputSystem;

[Singleton]
public class KeyboardStateEnvNewSystem : Interfaces.VirtualEnvironment.InputState, IKeyboardStateEnvNewSystem
{
    public List<Key> HeldKeys { get; } = new();

    public void Hold(Key key)
    {
        if (HeldKeys.Contains(key)) return;
        HeldKeys.Add(key);
    }

    public void Release(Key key)
    {
        HeldKeys.Remove(key);
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