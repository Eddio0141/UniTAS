namespace UniTASPlugin.Movie.ScriptEngine.Exceptions.ParseExceptions;

public class UsingLoopActionOutsideOfLoopException : MovieParseException
{
    public UsingLoopActionOutsideOfLoopException(string actionName) : base(
        $"The action {actionName} is not used inside a loop")
    {
    }
}