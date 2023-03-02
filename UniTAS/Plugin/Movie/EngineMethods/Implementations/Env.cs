using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.GameEnvironment;

namespace UniTAS.Plugin.Movie.EngineMethods.Implementations;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Env : EngineMethodClass
{
    private readonly VirtualEnvironment _virtualEnvironment;

    public Env(VirtualEnvironment virtualEnvironment)
    {
        _virtualEnvironment = virtualEnvironment;
    }

    public float Fps
    {
        get => 1f / _virtualEnvironment.FrameTime;
        set => _virtualEnvironment.FrameTime = 1f / value;
    }

    public float Frametime
    {
        get => _virtualEnvironment.FrameTime;
        set => _virtualEnvironment.FrameTime = value;
    }
}