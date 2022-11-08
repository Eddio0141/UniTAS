namespace UniTASPlugin.Movie.DefaultParsers.DefaultMovieScriptParser;

public class VariableExpression : ExpressionBase
{
    public string Name { get; }

    public VariableExpression(string name)
    {
        Name = name;
    }
}