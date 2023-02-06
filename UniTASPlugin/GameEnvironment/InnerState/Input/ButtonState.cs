using System.Collections.Generic;

namespace UniTASPlugin.GameEnvironment.InnerState.Input;

public class ButtonState : InputDeviceBase
{
    public List<string> Buttons { get; } = new();
    public List<string> ButtonsDown { get; } = new();
    public List<string> ButtonsUp { get; } = new();
    private readonly List<string> _buttonsPrev = new();

    public override void Update()
    {
        ButtonsDown.Clear();
        ButtonsUp.Clear();

        for (var i = 0; i < _buttonsPrev.Count; i++)
        {
            var button = _buttonsPrev[i];
            if (Buttons.Contains(button)) continue;
            ButtonsUp.Add(button);
            _buttonsPrev.RemoveAt(i);
            i--;
        }

        foreach (var button in Buttons)
        {
            if (_buttonsPrev.Contains(button)) continue;
            ButtonsDown.Add(button);
            _buttonsPrev.Add(button);
        }
    }

    public override void ResetState()
    {
        Buttons.Clear();
        ButtonsDown.Clear();
        ButtonsUp.Clear();
        _buttonsPrev.Clear();
    }
}