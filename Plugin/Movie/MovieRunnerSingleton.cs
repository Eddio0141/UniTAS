using System;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ScriptEngine;
using UniTASPlugin.UpdateHelper;

namespace UniTASPlugin.Movie;

public class MovieRunnerSingleton : IOnUpdate
{
    public static MovieRunnerSingleton Instance { get; } = new MovieRunnerSingleton();

    public MovieRunnerSingleton()
    {
        IsRunning = false;
    }

    private MovieScriptEngine _engine;
    public bool IsRunning { get; private set; }

    public void RunFromPath(string path)
    {
        // TODO load text from path
        var pathText = path;

        // parse
        var movie = MovieParseProcessor.Parse(pathText);

        // warnings

        // TODO apply environment

        // init engine
        _engine = new MovieScriptEngine(movie.Script);

        // set env
        GameEnvironmentSingleton.Instance.InputState.ResetStates();
        GameEnvironmentSingleton.Instance.RunVirtualEnvironment = true;
        // TODO other stuff like save state load, reset, hide cursor, etc
        // TODO handle empty movie

        throw new NotImplementedException();
    }

    public void Update(float deltaTime)
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
            MovieEnd();
            return;
        }

        // TODO
        _engine.CurrentState();
        _engine.AdvanceFrame();

        throw new NotImplementedException();
    }

    private void MovieEnd()
    {
        GameEnvironmentSingleton.Instance.RunVirtualEnvironment = false;
        // TODO set frameTime to 0
        throw new NotImplementedException();
    }
}