using UniTAS.Plugin.Movie.ValueTypes;

namespace UniTAS.Plugin.Movie.Parsers.MovieScriptParser.Expressions;

public class CastExpression : OperationExpression
{
    public BasicValueType ValueType { get; }

    public CastExpression(BasicValueType valueType) : base(OperationType.Cast)
    {
        ValueType = valueType;
    }
}