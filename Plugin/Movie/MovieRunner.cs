using System;
using UniTASPlugin.GameEnvironment.Interfaces;
using UniTASPlugin.Movie.ParseInterfaces;
using UniTASPlugin.Movie.ScriptEngine;

namespace UniTASPlugin.Movie;

public class MovieRunner
{
    private readonly IMovieParser _parser;
    private MovieScriptEngine _engine;
    public bool IsRunning { get; private set; }

    public MovieRunner(IMovieParser parser)
    {
        IsRunning = false;
        _parser = parser;
    }

    public void RunFromPath<TEnv>(string path, ref TEnv env)
    where TEnv :
        IRunVirtualEnvironmentProperty,
        IInputStateProperty
    {
        // TODO load text from path
        var pathText = path;

        // parse
        var movie = _parser.Parse(pathText);

        // warnings

        // TODO apply environment

        // init engine
        _engine = new MovieScriptEngine(movie.Script);

        // set env
        env.InputState.ResetStates();
        env.RunVirtualEnvironment = true;
        // TODO other stuff like save state load, reset, hide cursor, etc
        // TODO handle empty movie

        throw new NotImplementedException();
    }

    public void Update<TEnv>(ref TEnv env)
    where TEnv :
        IRunVirtualEnvironmentProperty,
        IInputStateProperty
    {
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

        if (_engine.MovieEnd)
        {
            IsRunning = false;
            MovieEnd(ref env);
            return;
        }

        // TODO
        _engine.CurrentState();
        _engine.AdvanceFrame();

        throw new NotImplementedException();
    }

    private void MovieEnd<TEnv>(ref TEnv env)
    where TEnv :
        IRunVirtualEnvironmentProperty
    {
        env.RunVirtualEnvironment = false;
        // TODO set frameTime to 0
        throw new NotImplementedException();
    }
}