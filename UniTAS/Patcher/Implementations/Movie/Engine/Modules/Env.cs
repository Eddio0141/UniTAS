using System;
using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Movie;
using UniTAS.Patcher.Models.Movie;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Movie;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.Movie.Engine.Modules;

[ExcludeRegisterIfTesting]
[Singleton]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Env : EngineMethodClass
{
    private readonly ITimeEnv _timeEnv;
    private readonly IMovieLogger _logger;
    private readonly IMovieRunner _movieRunner;

    private readonly bool _mobile = Application.platform is RuntimePlatform.Android or RuntimePlatform.IPhonePlayer;

    [MoonSharpHidden]
    public Env(ITimeEnv timeEnv, IMovieLogger logger, IMovieRunner movieRunner, IUpdateEvents updateEvents)
    {
        _timeEnv = timeEnv;
        _logger = logger;
        _movieRunner = movieRunner;
        movieRunner.OnMovieStart += UpdateLastTrackers;
        movieRunner.OnMovieStart += () =>
        {
            UpdateLastTrackers();
            updateEvents.OnLastUpdateActual += OnLastUpdateActual;
        };
        movieRunner.OnMovieEnd += () => { updateEvents.OnLastUpdateActual -= OnLastUpdateActual; };
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

    private void OnLastUpdateActual()
    {
        // check if game has changed either targetFrameRate or vSyncCount
        // either of these values can change at any time, so we need to check for changes
        if (_lastTargetFrameRate != Application.targetFrameRate || _lastVSyncCount != QualitySettings.vSyncCount)
        {
            SetFrametime(_timeEnv.FrameTime);
        }
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