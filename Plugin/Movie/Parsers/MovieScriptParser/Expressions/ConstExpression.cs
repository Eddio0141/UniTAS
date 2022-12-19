using UniTASPlugin.Movie.MovieRunner.ValueTypes;

namespace UniTASPlugin.Movie.MovieRunner.Parsers.MovieScriptParser.Expressions;

public class ConstExpression : Expression
{
    public ValueType Value { get; }

    public ConstExpression(ValueType value)
    {
        Value = value;
    }
}