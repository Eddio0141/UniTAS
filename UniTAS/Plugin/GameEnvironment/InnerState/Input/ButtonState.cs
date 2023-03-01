using System.Collections.Generic;
using System.Collections.Immutable;

namespace UniTAS.Plugin.GameEnvironment.InnerState.Input;

public class ButtonState : InputDeviceBase
{
    public ImmutableList<string> Buttons => _buttons.ToImmutableList();
    public ImmutableList<string> ButtonsDown => _buttonsDown.ToImmutableList();
    public ImmutableList<string> ButtonsUp => _buttonsUp.ToImmutableList();

    private readonly List<string> _buttonsPrev = new();
    private readonly List<string> _buttons = new();
    private readonly List<string> _buttonsDown = new();
    private readonly List<string> _buttonsUp = new();

    public override void Update()
    {
        _buttonsDown.Clear();
        _buttonsUp.Clear();

        for (var i = 0; i < _buttonsPrev.Count; i++)
        {
            var button = _buttonsPrev[i];
            if (_buttons.Contains(button)) continue;
            _buttonsUp.Add(button);
            _buttonsPrev.RemoveAt(i);
            i--;
        }

        foreach (var button in _buttons)
        {
            if (_buttonsPrev.Contains(button)) continue;
            _buttonsDown.Add(button);
            _buttonsPrev.Add(button);
        }
    }

    public void Hold(string button)
    {
        if (_buttons.Contains(button)) return;
        _buttons.Add(button);
    }

    public void Release(string button)
    {
        _buttons.Remove(button);
    }

    public void Clear()
    {
        _buttons.Clear();
    }

    public override void ResetState()
    {
        _buttons.Clear();
        _buttonsDown.Clear();
        _buttonsUp.Clear();
        _buttonsPrev.Clear();
    }
}