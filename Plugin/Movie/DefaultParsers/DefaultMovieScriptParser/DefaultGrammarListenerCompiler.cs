using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.Movie.Models.Script;
using UniTASPlugin.Movie.ScriptEngine;
using UniTASPlugin.Movie.ScriptEngine.OpCodes;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Method;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.VariableSet;

namespace UniTASPlugin.Movie.DefaultParsers.DefaultMovieScriptParser;

public class DefaultGrammarListenerCompiler : MovieScriptDefaultGrammarBaseListener
{
    private readonly List<OpCodeBase> _mainBuilder = new();
    private readonly List<KeyValuePair<string, List<OpCodeBase>>> _builtMethods = new();
    private readonly Stack<KeyValuePair<string, List<OpCodeBase>>> _methodBuilders = new();
    private bool _buildingMethod;

    public ScriptModel Compile()
    {
        return new ScriptModel(new(null, _mainBuilder),
            new List<ScriptMethodModel>(_builtMethods.Select(x => new ScriptMethodModel(x.Key, x.Value))));
    }

    private void AddOpCode(OpCodeBase opCode)
    {
        if (_buildingMethod)
        {
            _methodBuilders.Peek().Value.Add(opCode);
        }
        else
        {
            _mainBuilder.Add(opCode);
        }
    }

    public override void EnterMethodDef(MovieScriptDefaultGrammarParser.MethodDefContext context)
    {
        var methodName = context.IDENTIFIER_STRING().GetText();
        _buildingMethod = true;
        _methodBuilders.Push(new KeyValuePair<string, List<OpCodeBase>>(methodName, new()));
    }
    
    public override void ExitMethodDef(MovieScriptDefaultGrammarParser.MethodDefContext context)
    {
        _builtMethods.Add(_methodBuilders.Pop());
        if (_methodBuilders.Count == 0)
        {
            _buildingMethod = false;
        }
    }

    public override void ExitMethodDefArgs(MovieScriptDefaultGrammarParser.MethodDefArgsContext context)
    {
        var argName = context.IDENTIFIER_STRING().GetText();
        AddOpCode(new PopArgOpCode(RegisterType.Temp));
        AddOpCode(new NewVariableOpCode(RegisterType.Temp, argName));
    }
}