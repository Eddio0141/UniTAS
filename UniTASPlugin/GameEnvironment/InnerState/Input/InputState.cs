using System;
using UniTASPlugin.GameRestart;

namespace UniTASPlugin.GameEnvironment.InnerState.Input;

// ReSharper disable once ClassNeverInstantiated.Global
public class InputState : IOnGameRestart
{
    public MouseState MouseState { get; } = new();
    public AxisState AxisState { get; } = new();
    public KeyboardState KeyboardState { get; } = new();
    public ButtonState ButtonState { get; } = new();

    public void OnGameRestart(DateTime startupTime)
    {
        ResetStates();
    }

    public void ResetStates()
    {
        MouseState.ResetState();
        AxisState.ResetState();
        KeyboardState.ResetState();
        ButtonState.ResetState();
    }

    public void Update()
    {
        MouseState.Update();
        AxisState.Update();
        KeyboardState.Update();
        ButtonState.Update();
    }
}