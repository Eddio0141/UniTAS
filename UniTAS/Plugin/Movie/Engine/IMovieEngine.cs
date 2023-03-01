using MoonSharp.Interpreter;

namespace UniTAS.Plugin.Movie.Engine;

public interface IMovieEngine
{
    void Update();
    bool Finished { get; }
    Script Script { get; }

    /// <summary>
    /// Registers a coroutine to be executed concurrently with the main coroutine
    /// </summary>
    /// <param name="coroutine">The method to be executed</param>
    /// <param name="postUpdate">If running after the main coroutine or before</param>
    /// <param name="defaultArgs">Default arguments to pass to the method, if any</param>
    /// <returns>Identifier for the registered concurrent method</returns>
    ConcurrentIdentifier RegisterConcurrent(DynValue coroutine, bool postUpdate, params DynValue[] defaultArgs);

    void UnregisterConcurrent(ConcurrentIdentifier identifier);
}