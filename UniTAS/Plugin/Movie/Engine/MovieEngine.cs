using MoonSharp.Interpreter;

namespace UniTAS.Plugin.Movie.Engine;

public class MovieEngine : IMovieEngine
{
    private readonly DynValue _coroutine;

    /// <summary>
    /// Creates a new MovieEngine from a coroutine
    /// </summary>
    /// <param name="engine">A coroutine for the movie</param>
    public MovieEngine(DynValue engine)
    {
        _coroutine = engine;
    }

    public void Update()
    {
        _coroutine.Coroutine.Resume();
    }

    public bool Finished => _coroutine.Coroutine.State == CoroutineState.Dead;
}