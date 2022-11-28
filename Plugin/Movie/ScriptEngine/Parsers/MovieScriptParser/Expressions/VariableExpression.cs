namespace UniTASPlugin.Movie.ScriptEngine.Parsers.MovieScriptParser.Expressions;

public class VariableExpression : ExpressionBase
{
    public string Name { get; }

    public VariableExpression(string name)
    {
        Name = name;
    }
}