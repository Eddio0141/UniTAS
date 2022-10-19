using System;
using UniTASPlugin.Movie.ScriptEngine;

namespace UniTASPlugin;

public class MovieRunnerSingleton
{
    public static MovieRunnerSingleton Instance { get; } = new MovieRunnerSingleton();

    private MovieScriptEngine _engine;

    public void RunFromPath(string path)
    {
        throw new NotImplementedException();
    }
}