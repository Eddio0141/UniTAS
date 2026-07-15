using System;

namespace UniTAS.Patcher.Exceptions.Movie.Runner;

public class MovieSetupException : Exception
{
    public MovieSetupException()
    {
    }

    public MovieSetupException(string message) : base(message)
    {
    }

    public MovieSetupException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
