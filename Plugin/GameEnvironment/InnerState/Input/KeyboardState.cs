using System.Collections.Generic;

namespace UniTASPlugin.GameEnvironment.InnerState.Input;

public class KeyboardState : InputDeviceBase
{
    public List<int> Keys { get; set; }
    public List<int> KeysDown { get; }
    public List<int> KeysUp { get; }
    private readonly List<int> _keysPrev;

    public KeyboardState()
    {
        Keys = new();
        KeysDown = new();
        KeysUp = new();
        _keysPrev = new();
    }

    public override void Update()
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

    public override void ResetState()
    {
        Keys.Clear();
        KeysDown.Clear();
        KeysUp.Clear();
        _keysPrev.Clear();
    }
}