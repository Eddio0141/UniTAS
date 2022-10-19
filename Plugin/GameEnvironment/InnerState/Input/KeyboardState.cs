using System.Collections.Generic;
using UniTASPlugin.UpdateHelper;
using UnityEngine;

namespace UniTASPlugin.GameEnvironment.InnerState.Input;

public class KeyboardState : IOnUpdate
{
    public List<KeyCode> Keys { get; set; }
    public List<KeyCode> KeysDown { get; }
    public List<KeyCode> KeysUp { get; }
    private readonly List<KeyCode> _keysPrev;

    public KeyboardState()
    {
        Keys = new List<KeyCode>();
        KeysDown = new List<KeyCode>();
        KeysUp = new List<KeyCode>();
        _keysPrev = new List<KeyCode>();
    }

    public void Update(float deltaTime)
    {
        KeysDown.Clear();
        KeysUp.Clear();

        foreach (var key in _keysPrev)
        {
            if (Keys.Contains(key)) continue;
            KeysUp.Add(key);
            _ = _keysPrev.Remove(key);
        }
        foreach (var key in Keys)
        {
            if (_keysPrev.Contains(key)) continue;
            KeysDown.Add(key);
            _keysPrev.Add(key);
        }
    }
}