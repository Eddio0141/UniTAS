using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.Movie;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Movie;

namespace UniTAS.Patcher.Implementations.Movie.Engine;

[Register]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class MovieEngine : IMovieEngine
{
    private readonly Dictionary<ConcurrentIdentifier, CoroutineHolder> _preUpdateCoroutines = new();
    private readonly Dictionary<ConcurrentIdentifier, CoroutineHolder> _postUpdateCoroutines = new();
    private DynValue _coroutine;
    private Script _script;

    private readonly IMovieLogger _movieLogger;
    private readonly ILogger _logger;

    public bool Finished { get; private set; }

    public Script Script
    {
        get => _script;
        set
        {
            // because script instance is changing, clear the coroutines
            _preUpdateCoroutines.Clear();
            _postUpdateCoroutines.Clear();
            _script = value;
        }
    }

    public PropertiesModel Properties { get; set; }

    /// <summary>
    /// Creates a new MovieEngine from a coroutine
    /// </summary>
    public MovieEngine(Script script, IMovieLogger movieLogger, ILogger logger)
    {
        Script = script;
        _movieLogger = movieLogger;
        _logger = logger;
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

        foreach (var pair in _preUpdateCoroutines)
        {
            var identifier = pair.Key;
            var coroutine = pair.Value;

            try
            {
                coroutine.Resume();
            }
            catch (Exception e)
            {
                ExceptionHandler(e);
                return;
            }

            if (!coroutine.RunOnce || !coroutine.Finished) continue;
            UnregisterConcurrent(identifier);
        }

        try
        {
            _coroutine.Coroutine.Resume();
        }
        catch (Exception e)
        {
            ExceptionHandler(e);
            return;
        }

        foreach (var pair in _postUpdateCoroutines)
        {
            var identifier = pair.Key;
            var coroutine = pair.Value;
            try
            {
                coroutine.Resume();
            }
            catch (Exception e)
            {
                ExceptionHandler(e);
                return;
            }

            if (!coroutine.RunOnce || !coroutine.Finished) continue;
            UnregisterConcurrent(identifier);
        }

        if (_coroutine.Coroutine.State == CoroutineState.Dead)
        {
            Finished = true;
        }
    }

    private void ExceptionHandler(Exception exception)
    {
        _movieLogger.LogError("Movie threw a runtime exception!");
        _movieLogger.LogError(exception.Message);
        _logger.LogDebug(exception);

        Finished = true;
    }

    public ConcurrentIdentifier RegisterConcurrent(bool postUpdate, DynValue coroutine, bool runOnce,
        params DynValue[] defaultArgs)
    {
        if (coroutine.Type != DataType.Function) return null;
        var coroutineWrap = new CoroutineHolder(this, coroutine, defaultArgs, runOnce);
        var identifier = new ConcurrentIdentifier(postUpdate);
        if (postUpdate)
        {
            _postUpdateCoroutines.Add(identifier, coroutineWrap);
        }
        else
        {
            coroutineWrap.Resume();
            if (coroutineWrap.RunOnce && coroutineWrap.Finished) return null;

            _preUpdateCoroutines.Add(identifier, coroutineWrap);
        }

        return identifier;
    }

    public void UnregisterConcurrent(ConcurrentIdentifier identifier)
    {
        if (identifier.PostUpdate)
        {
            _postUpdateCoroutines.Remove(identifier);
        }
        else
        {
            _preUpdateCoroutines.Remove(identifier);
        }
    }
}