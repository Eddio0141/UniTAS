using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace UniTAS.Plugin.Movie.Engine;

public partial class MovieEngine : IMovieEngine
{
    private readonly List<CoroutineHolder> _preUpdateCoroutines = new();
    private readonly List<CoroutineHolder> _postUpdateCoroutines = new();
    private DynValue _coroutine;
    private Script _script;
    private readonly List<ConcurrentIdentifier> _concurrentIdentifiers = new();

    public Script Script
    {
        get => _script;
        set
        {
            // because script instance is changing, clear the coroutines
            _preUpdateCoroutines.Clear();
            _postUpdateCoroutines.Clear();
            _concurrentIdentifiers.Clear();
            _script = value;
        }
    }

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
        if (Finished) return;
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

    public ConcurrentIdentifier RegisterConcurrent(DynValue coroutine, bool postUpdate, params DynValue[] defaultArgs)
    {
        if (coroutine.Type != DataType.Function) return null;
        var coroutineWrap = new CoroutineHolder(this, coroutine, defaultArgs);
        int index;
        if (postUpdate)
        {
            _postUpdateCoroutines.Add(coroutineWrap);

            index = _postUpdateCoroutines.Count - 1;
        }
        else
        {
            coroutineWrap.Resume();
            _preUpdateCoroutines.Add(coroutineWrap);

            index = _preUpdateCoroutines.Count - 1;
        }

        var identifier = new ConcurrentIdentifier(index, postUpdate);
        _concurrentIdentifiers.Add(identifier);

        return identifier;
    }

    public void UnregisterConcurrent(ConcurrentIdentifier identifier)
    {
        if (!_concurrentIdentifiers.Contains(identifier)) return;

        // remove
        if (identifier.PostUpdate)
        {
            _postUpdateCoroutines.RemoveAt(identifier.Index);
        }
        else
        {
            _preUpdateCoroutines.RemoveAt(identifier.Index);
        }

        _concurrentIdentifiers.Remove(identifier);

        // fix indexes
        foreach (var concurrentIdentifier in _concurrentIdentifiers)
        {
            if (concurrentIdentifier.PostUpdate != identifier.PostUpdate) continue;

            if (concurrentIdentifier.Index > identifier.Index)
            {
                concurrentIdentifier.Index--;
            }
        }
    }
}