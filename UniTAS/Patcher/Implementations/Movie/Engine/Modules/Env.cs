using System;
using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.DontRunIfPaused;
using UniTAS.Patcher.Interfaces.Movie;
using UniTAS.Patcher.Models.Movie;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Movie;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[ExcludeRegisterIfTesting]
[Singleton]
public class Env : EngineMethodClass, IOnLastUpdateActual
{
    private readonly ITimeEnv _timeEnv;
    private readonly IMovieLogger _logger;
    private readonly IMovieRunner _movieRunner;

    private readonly bool _mobile = Application.platform is RuntimePlatform.Android or RuntimePlatform.IPhonePlayer;

    [MoonSharpHidden]
    public Env(ITimeEnv timeEnv, IMovieLogger logger, IMovieRunner movieRunner, IMovieRunnerEvents movieEvents)
    {
        _timeEnv = timeEnv;
        _logger = logger;
        _movieRunner = movieRunner;
        movieEvents.OnMovieStart += OnMovieStart;
    }

    public double Fps
    {
        get => 1.0 / _timeEnv.FrameTime;
        set => SetFrametime(1.0 / value);
    }

    public double Frametime
    {
        get => _timeEnv.FrameTime;
        set => SetFrametime(value);
    }

    // Game FPS notes:
    // Application.targetFrameRate is the target frame rate for the application, -1 being platform's "default" frame rate.
    // Application.targetFrameRate is ignored completely by VR.
    // Application.targetFrameRate is ignored if QualitySettings.vSyncCount is set to a value other than 0.
    // QualitySettings.vSyncCount is the number of vertical syncs per frame, 0 by default.
    // The framerate outcome from QualitySettings.vSyncCount is Display Refresh Rate / QualitySettings.vSyncCount.
    // QualitySettings.vSyncCount is ignored completely by VR and Mobile.
    //
    // Default targetFrameRate on platforms:
    // Android: 30 fps
    // Other: unlimited
    //
    // Summary:
    // The FPS has a limit if
    // - If mobile and targetFrameRate is not set to -1
    // - If pc and targetFrameRate is not set to -1 and vSyncCount is 0

    private int _lastTargetFrameRate;
    private int _lastVSyncCount;

    // TODO set Screen.currentResolution refresh rate to movie's max achieving framerate
    private void SetFrametime(double value)
    {
        UpdateLastTrackers();

        var fps = 1.0 / value;
        if (Application.targetFrameRate != -1 && fps > Application.targetFrameRate &&
            (_mobile || (!_mobile && QualitySettings.vSyncCount == 0)))
        {
            _logger.LogWarning($"Target framerate is limited by the platform to {Application.targetFrameRate} fps");
            fps = Application.targetFrameRate;
        }

        _timeEnv.FrameTime = 1f / fps;
    }

    [MoonSharpHidden]
    public void OnLastUpdateActual()
    {
        if (_movieRunner.MovieEnd) return;

        // check if game has changed either targetFrameRate or vSyncCount
        // either of these values can change at any time, so we need to check for changes
        if (_lastTargetFrameRate != Application.targetFrameRate || _lastVSyncCount != QualitySettings.vSyncCount)
        {
            SetFrametime(_timeEnv.FrameTime);
        }
    }

    private void OnMovieStart()
    {
        UpdateLastTrackers();
    }

    private void UpdateLastTrackers()
    {
        _lastTargetFrameRate = Application.targetFrameRate;
        _lastVSyncCount = QualitySettings.vSyncCount;
    }

    public void Update_type(string updateTypeRaw)
    {
        UpdateType updateType;
        try
        {
            updateType = (UpdateType)Enum.Parse(typeof(UpdateType), updateTypeRaw, true);
        }
        catch (Exception)
        {
            _logger.LogWarning("Invalid update type, not changing");
            return;
        }

        _movieRunner.UpdateType = updateType;
    }
}