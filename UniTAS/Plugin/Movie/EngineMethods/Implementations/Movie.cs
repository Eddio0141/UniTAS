using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.MainThreadSpeedController;

namespace UniTAS.Plugin.Movie.EngineMethods.Implementations;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Movie : EngineMethodClass
{
    private readonly IMainThreadSpeedControl _mainThreadSpeedControl;

    public Movie(IMainThreadSpeedControl mainThreadSpeedControl)
    {
        _mainThreadSpeedControl = mainThreadSpeedControl;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public float Playback_speed
    {
        get => _mainThreadSpeedControl.SpeedMultiplier;
        set => _mainThreadSpeedControl.SpeedMultiplier = value;
    }
}