using System.Collections.Generic;
using System.Collections.Immutable;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.VirtualEnvironment;
using UniTAS.Plugin.Models.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;

namespace UniTAS.Plugin.Implementations.VirtualEnvironment.Input;

[Singleton]
public class KeyboardStateEnv : InputDevice, IKeyboardStateEnv
{
    public ImmutableList<Key> Keys => _keys.ToImmutableList();
    public ImmutableList<Key> KeysDown => _keysDown.ToImmutableList();
    public ImmutableList<Key> KeysUp => _keysUp.ToImmutableList();

    private readonly List<Key> _keysPrev;
    private readonly List<Key> _keys;
    private readonly List<Key> _keysDown;
    private readonly List<Key> _keysUp;

    public KeyboardStateEnv()
    {
        _keys = new();
        _keysDown = new();
        _keysUp = new();
        _keysPrev = new();
    }

    public void Hold(Key key)
    {
        if (_keys.Contains(key)) return;
        _keys.Add(key);
    }

    public void Release(Key key)
    {
        _keys.Remove(key);
    }

    public void Clear()
    {
        _keys.Clear();
    }

    protected override void Update()
    {
        _keysDown.Clear();
        _keysUp.Clear();

        for (var i = 0; i < _keysPrev.Count; i++)
        {
            var key = _keysPrev[i];
            if (_keys.Contains(key)) continue;
            _keysUp.Add(key);
            _keysPrev.RemoveAt(i);
            i--;
        }

        foreach (var key in _keys)
        {
            if (_keysPrev.Contains(key)) continue;
            _keysDown.Add(key);
            _keysPrev.Add(key);
        }
    }

    protected override void ResetState()
    {
        _keys.Clear();
        _keysDown.Clear();
        _keysUp.Clear();
        _keysPrev.Clear();
    }
}