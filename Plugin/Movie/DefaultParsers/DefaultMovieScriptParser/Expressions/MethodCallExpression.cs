namespace UniTASPlugin.Movie.DefaultParsers.DefaultMovieScriptParser.Expressions;

public class MethodCallExpression : ExpressionBase
{
    public string MethodName { get; }

    public MethodCallExpression(string methodName)
    {
        MethodName = methodName;
    }
}