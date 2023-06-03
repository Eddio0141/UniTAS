namespace UniTAS.Plugin.Exceptions.Movie.Parser;

public class NotReturningFunctionException : MovieEngineParserException
{
    public NotReturningFunctionException(string message) : base(message)
    {
    }
}