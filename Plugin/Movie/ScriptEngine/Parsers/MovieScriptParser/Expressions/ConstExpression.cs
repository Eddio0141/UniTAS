using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.Parsers.MovieScriptParser.Expressions;

public class ConstExpression : ExpressionBase
{
    public ValueType Value { get; }

    public ConstExpression(ValueType value)
    {
        Value = value;
    }
}