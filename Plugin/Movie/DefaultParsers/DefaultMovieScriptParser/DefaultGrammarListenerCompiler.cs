using System.Collections.Generic;
using UniTASPlugin.Movie.ScriptEngine.OpCodes;

namespace UniTASPlugin.Movie.DefaultParsers.DefaultMovieScriptParser;

public class DefaultGrammarListenerCompiler : MovieScriptDefaultGrammarBaseListener
{
    private List<OpCodeBase> _mainBuilder = new();
    private List<KeyValuePair<string, List<OpCodeBase>>> _builtMethods = new();
    private Stack<KeyValuePair<string, List<OpCodeBase>>> _methodBuilders = new();
    private bool _buildingMethod = false;

    private void AddOpCodes(IEnumerable<OpCodeBase> opCodes)
    {
        if (_buildingMethod)
        {
            _methodBuilders.Peek().Value.AddRange(opCodes);
        }
        else
        {
            _mainBuilder.AddRange(opCodes);
        }
    }

    public override void EnterMethodDef(MovieScriptDefaultGrammarParser.MethodDefContext context)
    {
        var methodName = context.IDENTIFIER_STRING().GetText();
        _buildingMethod = true;
        _methodBuilders.Push(new KeyValuePair<string, List<OpCodeBase>>(methodName, new()));

        // args
        context.methodDefArgs()
    }
}