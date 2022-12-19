namespace UniTASPlugin.Movie.MovieRunner.Parsers.MovieScriptParser.Expressions;

public class VariableExpression : Expression
{
    public string Name { get; }

    public VariableExpression(string name)
    {
        Name = name;
    }
}