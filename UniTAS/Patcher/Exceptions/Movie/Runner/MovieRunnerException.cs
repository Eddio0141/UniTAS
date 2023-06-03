using System;

namespace UniTAS.Patcher.Exceptions.Movie.Runner;

public abstract class MovieRunnerException : Exception
{
    protected MovieRunnerException(string message) : base($"Movie runner threw an exception: {message}")
    {
    }
}