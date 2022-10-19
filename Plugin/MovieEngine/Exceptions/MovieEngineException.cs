using System;

namespace UniTASPlugin.MovieEngine.Exceptions;

public class MovieEngineException : Exception
{
    public MovieEngineException(string message) : base($"Movie engine threw an exception: {message}") { }
}