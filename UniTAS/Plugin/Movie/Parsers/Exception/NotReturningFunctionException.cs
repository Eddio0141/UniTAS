namespace UniTAS.Plugin.Movie.Parsers.Exception;

public class NotReturningFunctionException : MovieEngineParserException
{
    public NotReturningFunctionException(string message) : base(message)
    {
    }
}