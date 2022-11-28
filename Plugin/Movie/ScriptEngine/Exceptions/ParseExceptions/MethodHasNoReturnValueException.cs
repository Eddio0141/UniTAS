namespace UniTASPlugin.Movie.ScriptEngine.Exceptions.ParseExceptions;

public class MethodHasNoReturnValueException : MovieParseException
{
    public MethodHasNoReturnValueException(string methodName) : base(
        $"Tried assigning value from {methodName} but contains no return value")
    {
    }
}