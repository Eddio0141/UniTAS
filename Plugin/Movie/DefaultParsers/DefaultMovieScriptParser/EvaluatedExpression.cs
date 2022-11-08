using System.Collections.Generic;
using UniTASPlugin.Movie.ScriptEngine.OpCodes;

namespace UniTASPlugin.Movie.DefaultParsers.DefaultMovieScriptParser;

public class EvaluatedExpression : ExpressionBase
{
    public List<OpCodeBase> OpCodes { get; }

    public EvaluatedExpression(List<OpCodeBase> opCodes)
    {
        OpCodes = opCodes;
    }
}