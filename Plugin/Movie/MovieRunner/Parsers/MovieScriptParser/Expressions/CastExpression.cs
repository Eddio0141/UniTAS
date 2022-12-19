using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.Parsers.MovieScriptParser.Expressions;

public class CastExpression : OperationExpression
{
    public BasicValueType ValueType { get; }

    public CastExpression(BasicValueType valueType) : base(OperationType.Cast)
    {
        ValueType = valueType;
    }
}