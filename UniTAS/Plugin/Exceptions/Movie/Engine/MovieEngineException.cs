using System;

namespace UniTAS.Plugin.Exceptions.Movie.Engine;

public abstract class MovieEngineException : Exception
{
    protected MovieEngineException(string message) : base(message)
    {
    }
}