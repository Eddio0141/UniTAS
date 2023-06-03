using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Plugin.Interfaces.Movie;
using UniTAS.Plugin.Services.Movie;

namespace UniTAS.Plugin.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Concurrent : EngineMethodClass
{
    private readonly IMovieEngine _engine;

    [MoonSharpHidden]
    public Concurrent(IMovieEngine engine)
    {
        _engine = engine;
    }

    public object Register(DynValue coroutine, bool postUpdate = false, params DynValue[] defaultArgs)
    {
        return _engine.RegisterConcurrent(postUpdate, coroutine, false, defaultArgs);
    }

    public object Register_once(DynValue coroutine, bool postUpdate = false, params DynValue[] defaultArgs)
    {
        return _engine.RegisterConcurrent(postUpdate, coroutine, true, defaultArgs);
    }

    public void Unregister(ConcurrentIdentifier identifier)
    {
        _engine.UnregisterConcurrent(identifier);
    }
}