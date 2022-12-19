namespace UniTASPlugin.Movie.MovieRunner.Exceptions.ParseExceptions;

public class UsingUndefinedMethodException : MovieParseException
{
    public UsingUndefinedMethodException(string method) : base($"Using an undefined method {method}")
    {
    }
}