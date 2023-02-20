namespace UniTAS.Plugin.Movie.Parsers.MovieScriptParser.Expressions;

public class VariableExpression : Expression
{
    public string Name { get; }

    public VariableExpression(string name)
    {
        Name = name;
    }
}