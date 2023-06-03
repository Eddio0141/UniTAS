using System;

namespace UniTAS.Patcher.Exceptions.Movie.Engine;

public abstract class MovieEngineException : Exception
{
    protected MovieEngineException(string message) : base(message)
    {
    }
}