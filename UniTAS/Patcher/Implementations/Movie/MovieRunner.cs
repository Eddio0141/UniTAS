using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Exceptions.Movie.Runner;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.DontRunIfPaused;
using UniTAS.Patcher.Interfaces.Events.Movie;
using UniTAS.Patcher.Models.Coroutine;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Models.Movie;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Movie;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.Movie;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Singleton(RegisterPriority.MovieRunner)]
public class MovieRunner : IMovieRunner, IOnInputUpdateActual, IMovieRunnerEvents
{
    private readonly IGameRestart _gameRestart;

    public bool MovieEnd { get; private set; } = true;
    private bool _setup;

    private readonly IMovieParser _parser;
    private IMovieEngine _engine;
    private readonly IMovieLogger _movieLogger;
    private readonly ILogger _logger;

    private readonly IOnMovieRunningStatusChange[] _onMovieRunningStatusChange;
    private readonly IOnMovieUpdate[] _onMovieUpdates;

    private readonly IVirtualEnvController _virtualEnvController;
    private readonly ITimeEnv _timeEnv;
    private readonly IRandomEnv _randomEnv;
    private readonly ICoroutine _coroutine;

    public UpdateType UpdateType { get; set; }

    public MovieRunner(IGameRestart gameRestart, IMovieParser parser, IMovieLogger movieLogger,
        IOnMovieRunningStatusChange[] onMovieRunningStatusChange,
        IVirtualEnvController virtualEnvController, ITimeEnv timeEnv, IRandomEnv randomEnv, ILogger logger,
        IOnMovieUpdate[] onMovieUpdates, ICoroutine coroutine)
    {
        _gameRestart = gameRestart;
        _parser = parser;
        _movieLogger = movieLogger;
        _onMovieRunningStatusChange = onMovieRunningStatusChange;
        _virtualEnvController = virtualEnvController;
        _timeEnv = timeEnv;
        _randomEnv = randomEnv;
        _logger = logger;
        _onMovieUpdates = onMovieUpdates;
        _coroutine = coroutine;

        _gameRestart.OnGameRestartResume += OnGameRestartResume;
    }

    public void RunFromInput(string input)
    {
        if (_setup) throw new MovieAlreadySettingUpException();
        _setup = true;

        if (!MovieEnd)
        {
            MovieRunningStatusChange(false);
        }

        Tuple<IMovieEngine, PropertiesModel> parsed;
        try
        {
            parsed = _parser.Parse(input);
        }
        catch (Exception e)
        {
            MovieRunningStatusChange(false);
            _setup = false;
            _movieLogger.LogError("Failed to run TAS movie, an exception was thrown!");
            _movieLogger.LogError(e.Message);
            _logger.LogDebug(e);

            return;
        }

        _engine = parsed.Item1;
        var properties = parsed.Item2;

        // set env from properties
        _virtualEnvController.RunVirtualEnvironment = true;

        if (properties.StartupProperties != null)
        {
            _logger.LogDebug($"Using startup property: {properties.StartupProperties}");
            _timeEnv.FrameTime = properties.StartupProperties.FrameTime;
            _randomEnv.StartUpSeed = properties.StartupProperties.Seed;
            _gameRestart.SoftRestart(properties.StartupProperties.StartTime);
        }

        UpdateType = properties.UpdateType;
        _logger.LogDebug($"set update type to {UpdateType}");
    }

    private void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        if (preMonoBehaviourResume) return;
        if (!_setup) return;
        _setup = false;
        MovieRunningStatusChange(true);
    }

    public void InputUpdateActual(bool fixedUpdate, bool newInputSystemUpdate)
    {
        if (MovieEnd) return;

        // skip if update type doesn't match current update type
        if (UpdateType != UpdateType.Both &&
            ((fixedUpdate && UpdateType != UpdateType.FixedUpdate) ||
             (!fixedUpdate && UpdateType != UpdateType.Update))
           )
        {
            RunUpdateEvents(fixedUpdate);
            return;
        }

        _engine.Update();
        RunUpdateEvents(fixedUpdate);

        if (_engine.Finished)
        {
            _timeEnv.FrameTime = 0;
            MovieRunningStatusChange(false);
            _coroutine.Start(FinishMovieCleanup());
            _movieLogger.LogInfo("movie end");
        }
    }

    private void RunUpdateEvents(bool fixedUpdate)
    {
        foreach (var onMovieUpdate in _onMovieUpdates)
        {
            onMovieUpdate.MovieUpdate(fixedUpdate);
        }
    }

    private void MovieRunningStatusChange(bool running)
    {
        if (running)
        {
            OnMovieStart?.Invoke();
        }
        else
        {
            OnMovieEnd?.Invoke();
        }

        MovieEnd = !running;
        foreach (var onMovieRunningStatusChange in _onMovieRunningStatusChange)
        {
            onMovieRunningStatusChange.OnMovieRunningStatusChange(running);
        }
    }

    private IEnumerator<CoroutineWait> FinishMovieCleanup()
    {
        yield return new WaitForUpdateUnconditional();
        _virtualEnvController.RunVirtualEnvironment = false;
    }

    public event Action OnMovieStart;
    public event Action OnMovieEnd;
}