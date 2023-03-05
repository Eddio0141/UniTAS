using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Plugin.GameEnvironment;
using UniTAS.Plugin.Logger;
using UnityEngine;

namespace UniTAS.Plugin.Movie.EngineMethods.Implementations;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Env : EngineMethodClass
{
    private readonly VirtualEnvironment _virtualEnvironment;
    private readonly IMovieLogger _logger;

    private readonly bool _mobile = Application.platform is RuntimePlatform.Android or RuntimePlatform.IPhonePlayer;

    [MoonSharpHidden]
    public Env(VirtualEnvironment virtualEnvironment, IMovieLogger logger)
    {
        _virtualEnvironment = virtualEnvironment;
        _logger = logger;
    }

    public float Fps
    {
        get => 1f / _virtualEnvironment.FrameTime;
        set => SetFrametime(1f / value);
    }

    public float Frametime
    {
        get => _virtualEnvironment.FrameTime;
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

    // TODO set Screen.currentResolution refresh rate to movie's max achieving framerate
    private void SetFrametime(float value)
    {
        var fps = 1f / value;
        if (Application.targetFrameRate != -1 && fps > Application.targetFrameRate &&
            _mobile || (!_mobile && QualitySettings.vSyncCount == 0))
        {
            _logger.LogWarning($"Target framerate is limited by the platform to {Application.targetFrameRate} fps");
            fps = Application.targetFrameRate;
        }

        _virtualEnvironment.FrameTime = 1f / fps;
    }
}