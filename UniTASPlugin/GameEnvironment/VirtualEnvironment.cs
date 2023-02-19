using System;
using System.Diagnostics;
using UniTASPlugin.GameEnvironment.InnerState;
using UniTASPlugin.GameEnvironment.InnerState.Input;
using UniTASPlugin.GameRestart;
using UniTASPlugin.GameRestart.EventInterfaces;

namespace UniTASPlugin.GameEnvironment;

/// <summary>
/// A class holding current virtual environment of the system the game is running on
/// </summary>
public class VirtualEnvironment : IOnGameRestartResume
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
    public InputState InputState { get; }

    public float FrameTime { get; set; }
    public GameTime GameTime { get; }

    public long Seed => GameTime.CurrentTime.Ticks;
    public Random SystemRandom { get; private set; }

    public UnityPaths UnityPaths { get; private set; }
    public string Username { get; set; } = "User";

    public VirtualEnvironment(InputState inputState, GameTime gameTime)
    {
        InputState = inputState;
        GameTime = gameTime;
    }

    public void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        SystemRandom = new((int)Seed);
        Trace.Write($"Setting System.Random seed to {Seed}");
        UnityPaths = new(Os, Username);
    }
}