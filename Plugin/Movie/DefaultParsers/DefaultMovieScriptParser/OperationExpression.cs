namespace UniTASPlugin.Movie.DefaultParsers.DefaultMovieScriptParser;

public class OperationExpression : ExpressionBase
{
    public OperationType Operation { get; }

    public OperationExpression(OperationType operation)
    {
        Operation = operation;
    }
}