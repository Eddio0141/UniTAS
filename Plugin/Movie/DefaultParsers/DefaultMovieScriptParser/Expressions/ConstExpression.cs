using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.DefaultParsers.DefaultMovieScriptParser.Expressions;

public class ConstExpression : ExpressionBase
{
    public ValueType Value { get; }

    public ConstExpression(ValueType value)
    {
        Value = value;
    }
}