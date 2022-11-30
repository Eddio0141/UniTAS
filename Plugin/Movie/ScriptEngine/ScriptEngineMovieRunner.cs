using System;
using UniTASPlugin.GameEnvironment.Interfaces;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods;
using UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine;

public class ScriptEngineMovieRunner : IMovieRunner
{
    public bool MovieEnd { get; private set; }
    public bool IsRunning => !MovieEnd;

    private readonly IMovieParser _parser;
    private readonly IGetDefinedMethods _getDefinedMethods;

    private ScriptEngineLowLevelEngine _engine;

    public ScriptEngineMovieRunner(IMovieParser parser, IGetDefinedMethods getDefinedMethods)
    {
        _parser = parser;
        _getDefinedMethods = getDefinedMethods;
    }

    public void RunFromPath<TEnv>(string path, ref TEnv env)
        where TEnv : IRunVirtualEnvironmentProperty, IInputStateProperty
    {
        // TODO load text from path
        var pathText = path;

        // parse
        var movie = _parser.Parse(pathText);

        // warnings

        // TODO apply environment

        // init engine
        _engine = new ScriptEngineLowLevelEngine(movie.Script, _getDefinedMethods);

        // set env
        env.InputState.ResetStates();
        env.RunVirtualEnvironment = true;
        // TODO other stuff like save state load, reset, hide cursor, etc
        // TODO handle empty movie

        MovieEnd = false;

        // TODO do we advance frame at the start?
        _engine.ExecUntilStop();
        throw new NotImplementedException();
    }

    public void Update<TEnv>(ref TEnv env)
        where TEnv :
        IRunVirtualEnvironmentProperty,
        IInputStateProperty
    {
        if (!IsRunning)
            return;

        _engine.ExecUntilStop();

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
            AtMovieEnd(ref env);
            return;
        }

        _engine.ExecUntilStop();

        throw new NotImplementedException();
    }

    private void AtMovieEnd<TEnv>(ref TEnv env)
        where TEnv :
        IRunVirtualEnvironmentProperty
    {
        env.RunVirtualEnvironment = false;
        // TODO set frameTime to 0
        throw new NotImplementedException();
    }

    public void AdvanceFrame()
    {
        throw new NotImplementedException();
    }
}