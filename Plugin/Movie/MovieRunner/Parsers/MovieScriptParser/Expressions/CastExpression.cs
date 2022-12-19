using UniTASPlugin.Movie.MovieRunner.ValueTypes;

namespace UniTASPlugin.Movie.MovieRunner.Parsers.MovieScriptParser.Expressions;

public class CastExpression : OperationExpression
{
    public BasicValueType ValueType { get; }

    public CastExpression(BasicValueType valueType) : base(OperationType.Cast)
    {
        ValueType = valueType;
    }
}