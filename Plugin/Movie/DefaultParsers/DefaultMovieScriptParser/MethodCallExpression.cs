namespace UniTASPlugin.Movie.DefaultParsers.DefaultMovieScriptParser;

public class MethodCallExpression : ExpressionBase
{
    public string MethodName { get; }

    public MethodCallExpression(string methodName)
    {
        MethodName = methodName;
    }
}