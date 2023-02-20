namespace UniTAS.Plugin.Movie.Exceptions.ParseExceptions;

public class UsingLoopActionOutsideOfLoopException : MovieParseException
{
    public UsingLoopActionOutsideOfLoopException(string actionName) : base(
        $"The action {actionName} is not used inside a loop")
    {
    }
}