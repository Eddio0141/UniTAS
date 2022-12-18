using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.FixedUpdateSync;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods;
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
    private bool _finished;

    public void RunFromInput(string input)
    {
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

        // set env
        // TODO apply environment
        var env = _virtualEnvironmentFactory.GetVirtualEnv();
        env.RunVirtualEnvironment = true;

        if (startupProperties != null)
        {
            env.FrameTime = startupProperties.FrameTime;
            _gameRestart.SoftRestart(startupProperties.StartTime);
        }

        // TODO other stuff like save state load, hide cursor, etc

        _finished = false;
        _syncFixedUpdate.OnSync(() => MovieEnd = false);
    }

    public void Update()
    {
        if (MovieEnd)
        {
            if (_finished)
            {
                var env = _virtualEnvironmentFactory.GetVirtualEnv();
                env.RunVirtualEnvironment = false;
                _finished = false;
            }

            return;
        }

        ConcurrentRunnersPreUpdate();
        _engine.ExecUntilStop(this);
        ConcurrentRunnersPostUpdate();

        if (_engine.FinishedExecuting)
        {
            MovieEnd = true;
            AtMovieEnd();
        }
    }

    private void AtMovieEnd()
    {
        var env = _virtualEnvironmentFactory.GetVirtualEnv();
        env.FrameTime = 0;
        _finished = true;
    }
}