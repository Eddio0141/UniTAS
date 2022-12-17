using System.Collections.Generic;
using System.Linq;
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

    private ScriptEngineLowLevelEngine _engine;
    private ScriptModel _mainScript;

    public ScriptEngineMovieRunner(IMovieParser parser, IEnumerable<EngineExternalMethod> externMethods,
        IVirtualEnvironmentFactory vEnvFactory, IGameRestart gameRestart)
    {
        _parser = parser;
        _externalMethods = externMethods.ToArray();
        _virtualEnvironmentFactory = vEnvFactory;
        _gameRestart = gameRestart;
    }

    public bool IsRunning => !MovieEnd;
    public bool MovieEnd { get; private set; } = true;

    public void RunFromInput(string input)
    {
        // parse
        var movie = _parser.Parse(input);
        _mainScript = movie.Script;
        var properties = movie.Properties;
        var startupProperties = properties.StartupProperties;

        // TODO warnings

        // init engine
        _engine = new(_mainScript, _externalMethods);

        // set env
        // TODO apply environment
        var env = _virtualEnvironmentFactory.GetVirtualEnv();
        env.RunVirtualEnvironment = true;
        if (startupProperties != null)
        {
            _gameRestart.SoftRestart(startupProperties.StartTime);
            env.FrameTime = startupProperties.FrameTime;
        }

        // TODO other stuff like save state load, hide cursor, etc

        MovieEnd = false;
    }

    public void Update()
    {
        if (MovieEnd)
            return;

        ConcurrentRunnersPreUpdate();
        _engine.ExecUntilStop(this);
        ConcurrentRunnersPostUpdate();

        // TODO input handle
        /*MouseState.Position = new Vector2(fb.Mouse.X, fb.Mouse.Y);
        MouseState.LeftClick = fb.Mouse.Left;
        MouseState.RightClick = fb.Mouse.Right;
        MouseState.MiddleClick = fb.Mouse.Middle;

        List<string> axisMoveSetDefault = new();
        foreach (var pair in AxisState.Values)
        {
            var key = pair.Key;
            if (!fb.Axises.AxisMove.ContainsKey(key))
                axisMoveSetDefault.Add(key);
        }
        foreach (var key in axisMoveSetDefault)
        {
            if (AxisState.Values.ContainsKey(key))
                AxisState.Values[key] = default;
            else
                AxisState.Values.Add(key, default);
        }
        foreach (var axisValue in fb.Axises.AxisMove)
        {
            var axis = axisValue.Key;
            var value = axisValue.Value;
            if (AxisState.Values.ContainsKey(axis))
            {
                AxisState.Values[axis] = value;
            }
            else
            {
                AxisState.Values.Add(axis, value);
            }
        }*/

        if (_engine.FinishedExecuting)
        {
            MovieEnd = true;
            AtMovieEnd();
        }
    }

    private void AtMovieEnd()
    {
        var env = _virtualEnvironmentFactory.GetVirtualEnv();
        env.RunVirtualEnvironment = false;
        // TODO TimeWrap.CaptureFrameTime = 0;
    }
}