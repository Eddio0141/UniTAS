namespace UniTASPlugin.Movie.DefaultParsers.DefaultMovieScriptParser.Expressions;

public class VariableExpression : ExpressionBase
{
    public string Name { get; }

    public VariableExpression(string name)
    {
        Name = name;
    }
}