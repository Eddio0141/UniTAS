namespace UniTASPlugin.Movie.MovieRunner.Exceptions.ParseExceptions;

public class InvokingArgsNotMatchingMethodDefException : MovieParseException
{
    public InvokingArgsNotMatchingMethodDefException(string method, int expected, int got) : base(
        $"Arguments doesn't match the defined arg count for calling method {method}, expected {expected}, got {got}")
    {
    }
}