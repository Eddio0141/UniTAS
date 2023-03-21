using System;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;

namespace UniTAS.Plugin.Services.VirtualEnvironment.InnerState.Input;

// ReSharper disable once ClassNeverInstantiated.Global
public class InputState : IOnGameRestart, IOnPreUpdates
{
    public MouseState MouseState { get; } = new();
    public AxisState AxisState { get; } = new();
    public KeyboardState KeyboardState { get; } = new();
    public ButtonState ButtonState { get; } = new();

    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
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

    public void PreUpdate()
    {
        MouseState.Update();
        AxisState.Update();
        KeyboardState.Update();
        ButtonState.Update();
    }
}