using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace UniTAS.Plugin.Movie.Engine;

public partial class MovieEngine : IMovieEngine
{
    private readonly List<CoroutineHolder> _preUpdateCoroutines = new();
    private readonly List<CoroutineHolder> _postUpdateCoroutines = new();
    private DynValue _coroutine;

    public Script Script { get; }

    /// <summary>
    /// Creates a new MovieEngine from a coroutine
    /// </summary>
    /// <param name="script">Script the coroutine lives in</param>
    public MovieEngine(Script script)
    {
        Script = script;
    }

    /// <summary>
    /// Initializes the engine with a coroutine method
    /// No need to manually create a coroutine via script.CreateCoroutine
    /// </summary>
    /// <param name="coroutine">The coroutine</param>
    public void InitCoroutine(DynValue coroutine)
    {
        if (_coroutine != null || coroutine.Type != DataType.Function) return;
        _coroutine = Script.CreateCoroutine(coroutine);
    }

    public void Update()
    {
        foreach (var coroutine in _preUpdateCoroutines)
        {
            coroutine.Resume();
        }

        _coroutine.Coroutine.Resume();

        foreach (var coroutine in _postUpdateCoroutines)
        {
            coroutine.Resume();
        }
    }

    public bool Finished => _coroutine.Coroutine.State == CoroutineState.Dead;

    public void RegisterPreUpdate(DynValue coroutine)
    {
        if (coroutine.Type != DataType.Function) return;
        _preUpdateCoroutines.Add(new(this, coroutine));
    }

    public void RegisterPostUpdate(DynValue coroutine)
    {
        if (coroutine.Type != DataType.Function) return;
        _postUpdateCoroutines.Add(new(this, coroutine));
    }
}