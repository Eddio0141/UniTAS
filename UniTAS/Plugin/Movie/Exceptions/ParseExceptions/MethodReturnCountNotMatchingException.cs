namespace UniTAS.Plugin.Movie.Exceptions.ParseExceptions;

public class MethodReturnCountNotMatchingException : MovieParseException
{
    public MethodReturnCountNotMatchingException() : base("Return expression count is not matching the others")
    {
    }
}