namespace UniTAS.Plugin.Movie.Exceptions.ParseExceptions;

public class MethodHasNoReturnValueException : MovieParseException
{
    public MethodHasNoReturnValueException(string methodName) : base(
        $"Tried assigning value from {methodName} but contains no return value")
    {
    }
}