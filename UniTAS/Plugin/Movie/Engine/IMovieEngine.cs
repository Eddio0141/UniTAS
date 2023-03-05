using MoonSharp.Interpreter;
using UniTAS.Plugin.Movie.MovieModels.Properties;

namespace UniTAS.Plugin.Movie.Engine;

public interface IMovieEngine
{
    void Update();
    bool Finished { get; }
    Script Script { get; }
    PropertiesModel Properties { get; }

    /// <summary>
    /// Registers a coroutine to be executed concurrently with the main coroutine
    /// </summary>
    /// <param name="postUpdate">If running after the main coroutine or before</param>
    /// <param name="coroutine">The method to be executed</param>
    /// <param name="runOnce">If coroutine should just run once</param>
    /// <param name="defaultArgs">Default arguments to pass to the method, if any</param>
    /// <returns>Identifier for the registered concurrent method</returns>
    ConcurrentIdentifier RegisterConcurrent(bool postUpdate, DynValue coroutine, bool runOnce,
        params DynValue[] defaultArgs);

    void UnregisterConcurrent(ConcurrentIdentifier identifier);
}