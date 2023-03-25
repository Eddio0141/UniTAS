using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;
using UniTAS.Plugin.Services.VirtualEnvironment.InnerState;
using UniTAS.Plugin.Services.VirtualEnvironment.InnerState.Input;

namespace UniTAS.Plugin.Services.VirtualEnvironment;

/// <summary>
///     A class holding current virtual environment of the system the game is running on
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class VirtualEnvironment : IOnGameRestartResume
{
    private readonly IConfig _config;
    private float _frameTime;
    private bool _runVirtualEnvironment;
    private readonly RandomEnv _randomEnv;

    public VirtualEnvironment(InputState inputState, GameTime gameTime, IConfig config)
    {
        InputState = inputState;
        GameTime = gameTime;
        _config = config;

        FrameTime = 0f;
        _randomEnv = new RandomEnv(this);
    }

    public bool RunVirtualEnvironment
    {
        get => _runVirtualEnvironment;
        set
        {
            if (_runVirtualEnvironment) InputState.ResetStates();

            _runVirtualEnvironment = value;
        }
    }

    public Os Os { get; set; } = Os.Windows;
    public WindowState WindowState { get; } = new(1920, 1080, false, true);
    public InputState InputState { get; }

    public float FrameTime
    {
        get => _frameTime;
        set
        {
            if (value <= 0) value = 1f / _config.DefaultFps;

            _frameTime = value;
        }
    }

    public GameTime GameTime { get; }

    public UnityPaths UnityPaths { get; private set; }
    public string Username { get; set; } = "User";

    public void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        RandomEnv.SystemRandom = new((int)RandomEnv.Seed);
        Trace.Write($"Setting System.Random seed to {RandomEnv.Seed}");
        UnityPaths = new(Os, Username);
    }
}