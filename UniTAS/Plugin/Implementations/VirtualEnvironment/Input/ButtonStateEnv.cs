using System.Collections.Generic;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;

namespace UniTAS.Plugin.Implementations.VirtualEnvironment.Input;

[Singleton]
public class ButtonStateEnv : InputDevice, IButtonStateEnv
{
    public List<string> Buttons { get; } = new();
    public List<string> ButtonsDown { get; } = new();
    public List<string> ButtonsUp { get; } = new();

    private readonly List<string> _buttonsPrev = new();

    protected override void Update()
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

    public void Hold(string button)
    {
        if (Buttons.Contains(button)) return;
        Buttons.Add(button);
    }

    public void Release(string button)
    {
        Buttons.Remove(button);
    }

    public void Clear()
    {
        Buttons.Clear();
    }

    protected override void ResetState()
    {
        Buttons.Clear();
        ButtonsDown.Clear();
        ButtonsUp.Clear();
        _buttonsPrev.Clear();
    }
}