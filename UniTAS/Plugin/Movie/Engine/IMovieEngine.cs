using MoonSharp.Interpreter;

namespace UniTAS.Plugin.Movie.Engine;

public interface IMovieEngine
{
    void Update();
    bool Finished { get; }
    Script Script { get; }
    void RegisterPreUpdate(DynValue coroutine);
    void RegisterPostUpdate(DynValue coroutine);
}