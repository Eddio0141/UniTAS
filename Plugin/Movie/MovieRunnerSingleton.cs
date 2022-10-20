using System;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.Factories;
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
        var movie = MovieFactory.ParseFromText(pathText);

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