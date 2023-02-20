using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UniTAS.Plugin.FixedUpdateSync;
using UniTAS.Plugin.GameEnvironment;
using UniTAS.Plugin.GameRestart;
using UniTAS.Plugin.Interfaces.Update;
using UniTAS.Plugin.Movie.EngineMethods;
using UniTAS.Plugin.Movie.Exceptions.ScriptEngineExceptions;
using UniTAS.Plugin.Movie.LowLevel;
using UniTAS.Plugin.Movie.MovieModels;
using UniTAS.Plugin.Movie.MovieModels.Script;
using UniTAS.Plugin.Movie.ParseInterfaces;

namespace UniTAS.Plugin.Movie;

public partial class MovieRunner : IMovieRunner, IOnPreUpdates
{
    private readonly List<LowLevelEngine> _concurrentRunnersPostUpdate = new();
    private readonly List<LowLevelEngine> _concurrentRunnersPreUpdate = new();
    private readonly EngineExternalMethod[] _externalMethods;

    private readonly IMovieParser _parser;

    private readonly VirtualEnvironment _virtualEnvironment;
    private readonly IGameRestart _gameRestart;

    private readonly ISyncFixedUpdate _syncFixedUpdate;

    private LowLevelEngine _engine;
    private ScriptModel _mainScript;

    public MovieRunner(IMovieParser parser, IEnumerable<EngineExternalMethod> externMethods,
        VirtualEnvironment vEnv, IGameRestart gameRestart, ISyncFixedUpdate syncFixedUpdate)
    {
        _parser = parser;
        _externalMethods = externMethods.ToArray();
        _virtualEnvironment = vEnv;
        _gameRestart = gameRestart;
        _syncFixedUpdate = syncFixedUpdate;
    }

    public bool IsRunning => !MovieEnd;
    public bool MovieEnd { get; private set; } = true;
    private bool _cleanUp;
    private bool _setup;

    public ulong FrameCount { get; private set; }

    public void RunFromInput(string input)
    {
        if (IsRunning || _setup) throw new MovieAlreadyRunningException();

        _setup = true;

        // parse
        MovieModel movie;
        try
        {
            movie = _parser.Parse(input);
        }
        catch (Exception)
        {
            _setup = false;
            throw;
        }

        _mainScript = movie.Script;
        var properties = movie.Properties;
        var startupProperties = properties.StartupProperties;

        // TODO warnings

        // init engine
        _concurrentRunnersPostUpdate.Clear();
        _concurrentRunnersPreUpdate.Clear();
        _engine = new(_mainScript, _externalMethods);

        // set env from properties
        // TODO apply environment
        _virtualEnvironment.RunVirtualEnvironment = true;

        if (startupProperties != null)
        {
            Trace.Write($"Using startup property: {startupProperties}");
            _virtualEnvironment.FrameTime = startupProperties.FrameTime;
            _gameRestart.SoftRestart(startupProperties.StartTime);
        }

        // TODO other stuff like save state load, hide cursor, etc

        FrameCount = 0;
        _syncFixedUpdate.OnSync(() =>
        {
            if (_gameRestart.PendingRestart)
            {
                _syncFixedUpdate.OnSync(() => { MovieEnd = false; }, 1, 1);
            }
            else
            {
                MovieEnd = false;
            }
        }, 1);
    }

    public void PreUpdate()
    {
        if (_cleanUp)
        {
            _virtualEnvironment.RunVirtualEnvironment = false;
            _cleanUp = false;
            return;
        }

        if (MovieEnd) return;

        ConcurrentRunnersPreUpdate();
        _engine.ExecUntilStop(this);
        ConcurrentRunnersPostUpdate();

        if (_engine.FinishedExecuting)
        {
            AtMovieEnd();
        }

        FrameCount++;
    }

    private void AtMovieEnd()
    {
        _virtualEnvironment.FrameTime = 0;
        _cleanUp = true;
        _setup = false;
        MovieEnd = true;
    }
}