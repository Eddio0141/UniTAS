using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.Parsers.MovieScriptParser.Expressions;

public class ConstExpression : Expression
{
    public ValueType Value { get; }

    public ConstExpression(ValueType value)
    {
        Value = value;
    }
}