namespace UniTAS.Plugin.Movie.Parsers.Exceptions;

public class NotReturningFunctionException : MovieEngineParserException
{
    public NotReturningFunctionException(string message) : base(message)
    {
    }
}