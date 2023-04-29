using System.Collections.Generic;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Models.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input.NewInputSystem;

namespace UniTAS.Plugin.Implementations.VirtualEnvironment.InputState.NewInputSystem;

[Singleton]
public class KeyboardStateEnvNewSystem : Interfaces.VirtualEnvironment.InputState, IKeyboardStateEnvNewSystem
{
    private readonly List<Key> _heldKeys = new();

    public void Hold(Key key)
    {
        if (_heldKeys.Contains(key)) return;
        _heldKeys.Add(key);
    }

    public void Release(Key key)
    {
        _heldKeys.Remove(key);
    }

    public bool IsKeyHeld(Key key)
    {
        return _heldKeys.Contains(key);
    }

    public void Clear()
    {
        _heldKeys.Clear();
    }

    protected override void ResetState()
    {
        _heldKeys.Clear();
    }
}