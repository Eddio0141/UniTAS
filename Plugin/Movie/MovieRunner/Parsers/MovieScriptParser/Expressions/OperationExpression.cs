﻿namespace UniTASPlugin.Movie.MovieRunner.Parsers.MovieScriptParser.Expressions;

public class OperationExpression : Expression
{
    public OperationType Operation { get; }

    public OperationExpression(OperationType operation)
    {
        Operation = operation;
    }
}