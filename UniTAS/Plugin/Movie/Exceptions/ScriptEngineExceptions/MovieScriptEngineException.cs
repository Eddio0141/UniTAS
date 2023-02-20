using System;

namespace UniTAS.Plugin.Movie.Exceptions.ScriptEngineExceptions;

public class MovieScriptEngineException : Exception
{
    public MovieScriptEngineException(string message) : base($"Movie engine threw an exception: {message}")
    {
    }
}