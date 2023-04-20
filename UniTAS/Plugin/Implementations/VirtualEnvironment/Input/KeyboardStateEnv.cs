using System.Collections.Generic;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.VirtualEnvironment;
using UniTAS.Plugin.Models.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;

namespace UniTAS.Plugin.Implementations.VirtualEnvironment.Input;

[Singleton]
public class KeyboardStateEnv : InputDevice, IKeyboardStateEnv
{
    public List<Key> Keys { get; } = new();
    public List<Key> KeysDown { get; } = new();
    public List<Key> KeysUp { get; } = new();

    private readonly List<Key> _keysPrev = new();

    public void Hold(Key key)
    {
        if (Keys.Contains(key)) return;
        Keys.Add(key);
    }

    public void Release(Key key)
    {
        Keys.Remove(key);
    }

    public void Clear()
    {
        Keys.Clear();
    }

    protected override void Update()
    {
        KeysDown.Clear();
        KeysUp.Clear();

        for (var i = 0; i < _keysPrev.Count; i++)
        {
            var key = _keysPrev[i];
            if (Keys.Contains(key)) continue;
            KeysUp.Add(key);
            _keysPrev.RemoveAt(i);
            i--;
        }

        foreach (var key in Keys)
        {
            if (_keysPrev.Contains(key)) continue;
            KeysDown.Add(key);
            _keysPrev.Add(key);
        }
    }

    protected override void ResetState()
    {
        Keys.Clear();
        KeysDown.Clear();
        KeysUp.Clear();
        _keysPrev.Clear();
    }
}