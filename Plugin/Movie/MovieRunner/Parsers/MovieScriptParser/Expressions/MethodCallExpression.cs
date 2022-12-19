﻿namespace UniTASPlugin.Movie.MovieRunner.Parsers.MovieScriptParser.Expressions;

public class MethodCallExpression : Expression
{
    public string MethodName { get; }

    public MethodCallExpression(string methodName)
    {
        MethodName = methodName;
    }
}