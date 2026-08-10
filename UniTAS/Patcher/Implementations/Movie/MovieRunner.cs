using System;
using System.Collections.Generic;
using System.IO;
using UniTAS.Patcher.Exceptions.Movie.Runner;
using UniTAS.Patcher.External;
using UniTAS.Patcher.Implementations.Coroutine;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.Movie;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.DontRunIfPaused;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Models.Movie;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Movie;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.Movie;

[Singleton(RegisterPriority.MovieRunner)]
public class MovieRunner : IMovieRunner, IOnUpdateActual, IOnFixedUpdateActual, IMovieRunnerEvents
{
    private readonly IGameRestart _gameRestart;

    public bool MovieEnd { get; private set; } = true;
    public bool SetupOrMovieRunning { get; private set; }
    private bool _setup;

    private readonly IMovieParser _parser;
    private IMovieEngine _engine;
    public IMovieLogger MovieLogger { get; }
    private readonly ILogger _logger;

    private readonly IOnMovieUpdate[] _onMovieUpdates;

    private readonly IVirtualEnvController _virtualEnvController;
    private readonly ITimeEnv _timeEnv;
    private readonly IRandomEnv _randomEnv;
    private readonly ICoroutine _coroutine;
    private readonly IWindowEnv _windowEnv;

    public UpdateType UpdateType { get; set; }

    public MovieRunner(IGameRestart gameRestart, IMovieParser parser, IMovieLogger movieLogger,
        IOnMovieRunningStatusChange[] onMovieRunningStatusChange,
        IVirtualEnvController virtualEnvController, ITimeEnv timeEnv, IRandomEnv randomEnv, ILogger logger,
        IOnMovieUpdate[] onMovieUpdates, ICoroutine coroutine, IWindowEnv windowEnv)
    {
        _gameRestart = gameRestart;
        _parser = parser;
        MovieLogger = movieLogger;
        _virtualEnvController = virtualEnvController;
        _timeEnv = timeEnv;
        _randomEnv = randomEnv;
        _logger = logger;
        _onMovieUpdates = onMovieUpdates;
        _coroutine = coroutine;
        _windowEnv = windowEnv;

        _gameRestart.OnGameRestartResume += OnGameRestartResume;

        foreach (var e in onMovieRunningStatusChange)
        {
            OnMovieRunningStatusChange += e.OnMovieRunningStatusChange;
        }
    }

    const string DirMovieName = "movie.lua";
    const string DirFsName = "filesystem";

    public void RunFromPath(string path)
    {
        if (_setup) throw new MovieAlreadySettingUpException();
        _setup = true;
        SetupOrMovieRunning = true;

        StaticLogger.LogDebug($"thingy, {path}");
        string moviePath;
        string fsPath;
        if (Directory.Exists(path))
        {
            MovieLogger.LogInfo("Reading TAS from directory");
            moviePath = Path.Combine(path, DirMovieName);
            fsPath = Path.Combine(path, DirFsName);
        }
        else if (Path.GetExtension(path) == ".zip")
        {
            MovieLogger.LogInfo("Reading TAS from zip");
            var dest = UniTASPaths.UniqueTempDir();
            UniTasRs.extract_zip(path, dest);
            moviePath = Path.Combine(path, DirMovieName);
            fsPath = Path.Combine(path, DirFsName);
        }
        else if (Path.GetExtension(path) == ".lua")
        {
            MovieLogger.LogInfo("Reading TAS from lua");
            moviePath = path;
            fsPath = null;
        }
        else
        {
            throw new MovieSetupException("Unknown movie format, cannot play movie");
        }

        string input;
        try
        {
            input = File.ReadAllText(moviePath);
        }
        catch (Exception e)
        {
            throw new MovieSetupException($"Failed to read movie content from `{moviePath}`", e);
        }

        if (!MovieEnd)
        {
            MovieRunningStatusChange(false);
        }

        (IMovieEngine, PropertiesModel) parsed;
        try
        {
            parsed = _parser.Parse(input);
        }
        catch (Exception e)
        {
            // needed for setup event to be notified of failure
            MovieRunningStatusChange(false);
            _setup = false;
            MovieLogger.LogError("Failed to run TAS movie, an exception was thrown!");
            MovieLogger.LogError(e.Message);
            _logger.LogDebug(e);

            return;
        }

        _engine = parsed.Item1;
        var props = parsed.Item2;

        _logger.LogDebug($"Using startup property: {props}");

        UniTasRs.movie_start(fsPath, props.FsPassthrough, (nuint)props.FsPassthrough.Length);

        // set env from properties
        _timeEnv.FrameTime = props.FrameTime;
        _randomEnv.StartUpSeed = props.Seed;
        props.WindowState.SetWindowEnv(_windowEnv);

        // this runs after setting venv up
        _virtualEnvController.RunVirtualEnvironment = true;

        _gameRestart.SoftRestart(props.StartTime);

        UpdateType = props.UpdateType;
        _logger.LogDebug($"set update type to {UpdateType}");
    }

    private void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        if (preMonoBehaviourResume) return;
        if (!_setup) return;
        _setup = false;
        MovieRunningStatusChange(true);
    }

    public void UpdateActual()
    {
        if (MovieEnd) return;

        // skip if update type doesn't match current update type
        if (UpdateType is UpdateType.FixedUpdate)
        {
            RunUpdateEvents(false);
            return;
        }

        EngineUpdateAndCheck(false);
    }

    public void FixedUpdateActual()
    {
        if (MovieEnd) return;

        // skip if update type doesn't match current update type
        if (UpdateType is UpdateType.Update)
        {
            RunUpdateEvents(true);
            return;
        }

        EngineUpdateAndCheck(true);
    }

    private void EngineUpdateAndCheck(bool fixedUpdate)
    {
        _engine.Update();
        RunUpdateEvents(fixedUpdate);

        if (!_engine.Finished) return;

        _timeEnv.FrameTime = 0;
        MovieRunningStatusChange(false);
        _coroutine.Start(FinishMovieCleanup()).OnComplete += status =>
        {
            if (status.Exception != null)
                _logger.LogFatal($"exception occurs during movie runner coroutine: {status.Exception}");
        };
        MovieLogger.LogInfo("movie end");
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
        MovieEnd = !running;

        if (running)
        {
            OnMovieStart?.Invoke();
        }
        else
        {
            UniTasRs.movie_end();
            SetupOrMovieRunning = false;
            OnMovieEnd?.Invoke();
        }

        OnMovieRunningStatusChange?.Invoke(running);
    }

    private IEnumerable<CoroutineWait> FinishMovieCleanup()
    {
        yield return new WaitForUpdateUnconditional();
        _virtualEnvController.RunVirtualEnvironment = false;
    }

    public event Action OnMovieStart;
    public event Action OnMovieEnd;
    public event Action<bool> OnMovieRunningStatusChange;
}
