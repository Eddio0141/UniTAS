using UniTAS.Plugin.Movie.ValueTypes;

namespace UniTAS.Plugin.Movie.Parsers.MovieScriptParser.Expressions;

public class ConstExpression : Expression
{
    public ValueType Value { get; }

    public ConstExpression(ValueType value)
    {
        Value = value;
    }
}