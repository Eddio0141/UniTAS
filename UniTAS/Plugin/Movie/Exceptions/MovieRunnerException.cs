using System;

namespace UniTAS.Plugin.Movie.Exceptions;

public abstract class MovieRunnerException : Exception
{
    protected MovieRunnerException(string message) : base($"Movie runner threw an exception: {message}")
    {
    }
}