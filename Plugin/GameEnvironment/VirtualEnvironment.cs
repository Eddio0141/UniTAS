using System;
using UniTASPlugin.GameEnvironment.InnerState;
using UniTASPlugin.GameEnvironment.InnerState.Input;
using UniTASPlugin.GameRestart;

namespace UniTASPlugin.GameEnvironment;

/// <summary>
/// A class holding current virtual environment of the system the game is running on
/// </summary>
public class VirtualEnvironment : IOnGameRestart
{
    private bool _runVirtualEnvironment;

    public bool RunVirtualEnvironment
    {
        get => _runVirtualEnvironment;
        set
        {
            if (_runVirtualEnvironment)
            {
                InputState.ResetStates();
            }

            _runVirtualEnvironment = value;
        }
    }

    public Os Os { get; set; } = Os.Windows;
    public WindowState WindowState { get; } = new(1920, 1080, false, true);
    public InputState InputState { get; } = new();
    public float FrameTime { get; set; }
    public GameTime GameTime { get; } = new();

    public long Seed => GameTime.CurrentTime.Ticks;
    public Random SystemRandom { get; private set; } = new();

    public void OnGameRestart(DateTime startupTime)
    {
        GameTime.StartupTime = startupTime;
        SystemRandom = new((int)Seed);

        InputState.ResetStates();
    }
}