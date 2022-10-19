using System;

namespace UniTASPlugin.Movie.ScriptEngineExceptions;

public class MovieScriptEngineException : Exception
{
    public MovieScriptEngineException(string message) : base($"Movie engine threw an exception: {message}") { }
}