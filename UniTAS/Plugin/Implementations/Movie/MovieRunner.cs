using System;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Exceptions.Movie.Runner;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.DontRunIfPaused;
using UniTAS.Plugin.Models.DependencyInjection;
using UniTAS.Plugin.Models.Movie;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.Logging;
using UniTAS.Plugin.Services.Movie;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UniTAS.Plugin.Utils;

namespace UniTAS.Plugin.Implementations.Movie;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Singleton(RegisterPriority.MovieRunner)]
public class MovieRunner : IMovieRunner, IOnPreUpdatesActual
{
    private readonly IGameRestart _gameRestart;

    public bool MovieEnd { get; private set; } = true;
    private bool _cleanUp;
    private bool _setup;

    private readonly IMovieParser _parser;
    private IMovieEngine _engine;
    private readonly IMovieLogger _movieLogger;
    private readonly ILogger _logger;

    private readonly IOnMovieRunningStatusChange[] _onMovieRunningStatusChange;

    private readonly IVirtualEnvController _virtualEnvController;
    private readonly ITimeEnv _timeEnv;
    private readonly IRandomEnv _randomEnv;

    public MovieRunner(IGameRestart gameRestart, IMovieParser parser, IMovieLogger movieLogger,
        IOnMovieRunningStatusChange[] onMovieRunningStatusChange,
        IVirtualEnvController virtualEnvController, ITimeEnv timeEnv, IRandomEnv randomEnv, ILogger logger)
    {
        _gameRestart = gameRestart;
        _parser = parser;
        _movieLogger = movieLogger;
        _onMovieRunningStatusChange = onMovieRunningStatusChange;
        _virtualEnvController = virtualEnvController;
        _timeEnv = timeEnv;
        _randomEnv = randomEnv;
        _logger = logger;

        _gameRestart.OnGameRestartResume += OnGameRestartResume;
    }

    public void RunFromInput(string input)
    {
        if (!MovieEnd || _setup) throw new MovieAlreadyRunningException();

        _setup = true;

        Tuple<IMovieEngine, PropertiesModel> parsed;
        try
        {
            parsed = _parser.Parse(input);
        }
        catch (Exception e)
        {
            MovieRunningStatusChange(false);
            _setup = false;
            _movieLogger.LogError($"Failed to run TAS movie, an exception was thrown!");
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

        // TODO other stuff like save state load, hide cursor, etc
    }

    private void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        if (preMonoBehaviourResume) return;
        if (!_setup) return;
        _setup = false;
        MovieRunningStatusChange(true);
    }

    public void PreUpdateActual()
    {
        if (_cleanUp)
        {
            _virtualEnvController.RunVirtualEnvironment = false;
            _cleanUp = false;
            return;
        }

        if (MovieEnd) return;

        _engine.Update();

        if (_engine.Finished)
        {
            AtMovieEnd();
        }
    }

    private void AtMovieEnd()
    {
        _timeEnv.FrameTime = 0;
        _cleanUp = true;
        _setup = false;
        MovieRunningStatusChange(false);

        _movieLogger.LogInfo("movie end");
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

    public event Action OnMovieStart;
    public event Action OnMovieEnd;
}