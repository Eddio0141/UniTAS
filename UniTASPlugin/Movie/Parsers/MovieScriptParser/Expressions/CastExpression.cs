using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.Parsers.MovieScriptParser.Expressions;

public class CastExpression : OperationExpression
{
    public BasicValueType ValueType { get; }

    public CastExpression(BasicValueType valueType) : base(OperationType.Cast)
    {
        ValueType = valueType;
    }
}