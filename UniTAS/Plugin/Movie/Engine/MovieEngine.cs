using System.Collections.Generic;
using System.Linq;
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

        for (var i = 0; i < _preUpdateCoroutines.Count; i++)
        {
            var coroutine = _preUpdateCoroutines[i];
            coroutine.Resume();

            if (!coroutine.RunOnce || !coroutine.Finished) continue;
            UnregisterConcurrent(new(i, false));
            i--;
        }

        _coroutine.Coroutine.Resume();

        for (var i = 0; i < _postUpdateCoroutines.Count; i++)
        {
            var coroutine = _postUpdateCoroutines[i];
            coroutine.Resume();

            if (!coroutine.RunOnce || !coroutine.Finished) continue;
            UnregisterConcurrent(new(i, true));
            i--;
        }
    }

    public bool Finished => _coroutine.Coroutine.State == CoroutineState.Dead;

    public ConcurrentIdentifier RegisterConcurrent(bool postUpdate, DynValue coroutine, bool runOnce,
        params DynValue[] defaultArgs)
    {
        if (coroutine.Type != DataType.Function) return null;
        var coroutineWrap = new CoroutineHolder(this, coroutine, defaultArgs, runOnce);
        int index;
        if (postUpdate)
        {
            _postUpdateCoroutines.Add(coroutineWrap);

            index = _postUpdateCoroutines.Count - 1;
        }
        else
        {
            coroutineWrap.Resume();
            if (coroutineWrap.RunOnce && coroutineWrap.Finished) return null;

            _preUpdateCoroutines.Add(coroutineWrap);

            index = _preUpdateCoroutines.Count - 1;
        }

        var identifier = new ConcurrentIdentifier(index, postUpdate);
        _concurrentIdentifiers.Add(identifier);

        return identifier;
    }

    public void UnregisterConcurrent(ConcurrentIdentifier identifier)
    {
        var foundIdentifier = _concurrentIdentifiers.FirstOrDefault(x => x.Equals(identifier));
        if (foundIdentifier == null) return;

        // remove
        if (foundIdentifier.PostUpdate)
        {
            _postUpdateCoroutines.RemoveAt(foundIdentifier.Index);
        }
        else
        {
            _preUpdateCoroutines.RemoveAt(foundIdentifier.Index);
        }

        _concurrentIdentifiers.Remove(foundIdentifier);

        // fix indexes
        foreach (var concurrentIdentifier in _concurrentIdentifiers)
        {
            if (concurrentIdentifier.PostUpdate != foundIdentifier.PostUpdate) continue;

            if (concurrentIdentifier.Index > foundIdentifier.Index)
            {
                concurrentIdentifier.Index--;
            }
        }
    }
}