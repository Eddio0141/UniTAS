using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.FixedUpdateSync;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameRestart;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods;
using UniTASPlugin.Movie.ScriptEngine.Exceptions.ScriptEngineExceptions;
using UniTASPlugin.Movie.ScriptEngine.LowLevelEngine;
using UniTASPlugin.Movie.ScriptEngine.MovieModels.Script;
using UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;

namespace UniTASPlugin.Movie.ScriptEngine;

public partial class ScriptEngineMovieRunner : IMovieRunner
{
    private readonly List<ScriptEngineLowLevelEngine> _concurrentRunnersPostUpdate = new();
    private readonly List<ScriptEngineLowLevelEngine> _concurrentRunnersPreUpdate = new();
    private readonly EngineExternalMethod[] _externalMethods;

    private readonly IMovieParser _parser;

    private readonly IVirtualEnvironmentFactory _virtualEnvironmentFactory;
    private readonly IGameRestart _gameRestart;

    private readonly ISyncFixedUpdate _syncFixedUpdate;

    private ScriptEngineLowLevelEngine _engine;
    private ScriptModel _mainScript;

    public ScriptEngineMovieRunner(IMovieParser parser, IEnumerable<EngineExternalMethod> externMethods,
        IVirtualEnvironmentFactory vEnvFactory, IGameRestart gameRestart, ISyncFixedUpdate syncFixedUpdate)
    {
        _parser = parser;
        _externalMethods = externMethods.ToArray();
        _virtualEnvironmentFactory = vEnvFactory;
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
        if (IsRunning) throw new MovieAlreadyRunningException();
        if (_setup) throw new MovieAlreadyRunningException();

        _setup = true;

        // parse
        var movie = _parser.Parse(input);
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
        var env = _virtualEnvironmentFactory.GetVirtualEnv();
        env.RunVirtualEnvironment = true;

        if (startupProperties != null)
        {
            env.FrameTime = startupProperties.FrameTime;
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

    public void Update()
    {
        if (_cleanUp)
        {
            var env = _virtualEnvironmentFactory.GetVirtualEnv();
            env.RunVirtualEnvironment = false;
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
        var env = _virtualEnvironmentFactory.GetVirtualEnv();
        env.FrameTime = 0;
        _cleanUp = true;
        _setup = false;
        MovieEnd = true;
    }
}