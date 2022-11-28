namespace UniTASPlugin.Movie.ScriptEngine.Parsers.MovieScriptParser.Expressions;

public class MethodCallExpression : ExpressionBase
{
    public string MethodName { get; }

    public MethodCallExpression(string methodName)
    {
        MethodName = methodName;
    }
}