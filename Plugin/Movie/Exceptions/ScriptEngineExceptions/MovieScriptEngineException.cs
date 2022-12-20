using System;

namespace UniTASPlugin.Movie.Exceptions.ScriptEngineExceptions;

public class MovieScriptEngineException : Exception
{
    public MovieScriptEngineException(string message) : base($"Movie engine threw an exception: {message}")
    {
    }
}