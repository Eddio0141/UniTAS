using System;

namespace UniTAS.Plugin.Movie.Engine.Exceptions;

public abstract class MovieEngineException : Exception
{
    protected MovieEngineException(string message) : base(message)
    {
    }
}