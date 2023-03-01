using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;

namespace UniTAS.Plugin.GameEnvironment.InnerState.Input;

public class KeyboardState : InputDeviceBase
{
    public ImmutableList<KeyCode> Keys => _keys.ToImmutableList();
    public ImmutableList<KeyCode> KeysDown => _keysDown.ToImmutableList();
    public ImmutableList<KeyCode> KeysUp => _keysUp.ToImmutableList();

    private readonly List<KeyCode> _keysPrev;
    private readonly List<KeyCode> _keys;
    private readonly List<KeyCode> _keysDown;
    private readonly List<KeyCode> _keysUp;

    public KeyboardState()
    {
        _keys = new();
        _keysDown = new();
        _keysUp = new();
        _keysPrev = new();
    }

    public void Press(KeyCode key)
    {
        if (_keys.Contains(key)) return;
        _keys.Add(key);
    }

    public void Release(KeyCode key)
    {
        _keys.Remove(key);
    }

    public override void Update()
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

    public override void ResetState()
    {
        _keys.Clear();
        _keysDown.Clear();
        _keysUp.Clear();
        _keysPrev.Clear();
    }
}