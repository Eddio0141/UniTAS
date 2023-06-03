using System;

namespace UniTAS.Plugin.Exceptions.Movie.Runner;

public abstract class MovieRunnerException : Exception
{
    protected MovieRunnerException(string message) : base($"Movie runner threw an exception: {message}")
    {
    }
}