using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Plugin.Movie.Engine;

namespace UniTAS.Plugin.Movie.EngineMethods.Implementations;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Concurrent : EngineMethodClass
{
    private readonly IMovieEngine _engine;

    public Concurrent(IMovieEngine engine)
    {
        _engine = engine;
    }

    public override string ClassName => "concurrent";

    public object Register(DynValue coroutine, bool postUpdate = false, params DynValue[] defaultArgs)
    {
        return _engine.RegisterConcurrent(coroutine, postUpdate, defaultArgs);
    }

    public void Unregister(ConcurrentIdentifier identifier)
    {
        _engine.UnregisterConcurrent(identifier);
    }
}