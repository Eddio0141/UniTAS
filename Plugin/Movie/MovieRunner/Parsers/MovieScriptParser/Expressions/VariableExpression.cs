namespace UniTASPlugin.Movie.ScriptEngine.Parsers.MovieScriptParser.Expressions;

public class VariableExpression : Expression
{
    public string Name { get; }

    public VariableExpression(string name)
    {
        Name = name;
    }
}