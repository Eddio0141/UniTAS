using System;
using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.Movie.Models.Script;
using UniTASPlugin.Movie.ScriptEngine;
using UniTASPlugin.Movie.ScriptEngine.OpCodes;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Method;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.RegisterSet;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.VariableSet;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.DefaultParsers.DefaultMovieScriptParser;

public class DefaultGrammarListenerCompiler : MovieScriptDefaultGrammarBaseListener
{
    private readonly List<OpCodeBase> _mainBuilder = new();
    private readonly List<KeyValuePair<string, List<OpCodeBase>>> _builtMethods = new();
    private readonly Stack<KeyValuePair<string, List<OpCodeBase>>> _methodBuilders = new();
    private bool _buildingMethod;
    private RegisterType? _reserveTupleListRegister;
    private RegisterType _expressionTerminatorRegister;
    private readonly bool[] _reservedTempRegister = new bool[RegisterType.Temp5 - RegisterType.Temp + 1];

    public ScriptModel Compile()
    {
        return new ScriptModel(new(null, _mainBuilder),
            new List<ScriptMethodModel>(_builtMethods.Select(x => new ScriptMethodModel(x.Key, x.Value))));
    }

    private RegisterType AllocateTempRegister()
    {
        for (var i = 0; i < _reservedTempRegister.Length; i++)
        {
            var reserveStatus = _reservedTempRegister[i];
            if (reserveStatus) continue;
            _reservedTempRegister[i] = true;
            return RegisterType.Temp + i;
        }

        throw new InvalidOperationException("ran out of temp registers, should never happen");
    }

    private void DeallocateTempRegister(RegisterType register)
    {
        if (register is > RegisterType.Temp5 or < RegisterType.Temp)
        {
            throw new InvalidOperationException("Out of range register value");
        }

        _reservedTempRegister[(int)register] = false;
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

    public override void EnterExpressionTerminator(MovieScriptDefaultGrammarParser.ExpressionTerminatorContext context)
    {
        _expressionTerminatorRegister = AllocateTempRegister();
        if (context.variable() != null)
        {
            var variable = context.variable().IDENTIFIER_STRING().GetText();
            AddOpCode(new VarToRegisterOpCode(_expressionTerminatorRegister, variable));
        }
        else if (context.intType() != null)
        {
            var value = context.intType().INT().GetText();
            var valueParsed = int.Parse(value);
            AddOpCode(new ConstToRegisterOpCode(_expressionTerminatorRegister, new IntValueType(valueParsed)));
        }
        else if (context.floatType() != null)
        {
            var value = context.floatType().FLOAT().GetText();
            var valueParsed = float.Parse(value);
            AddOpCode(new ConstToRegisterOpCode(_expressionTerminatorRegister, new FloatValueType(valueParsed)));
        }
        else if (context.@bool() != null)
        {
            var value = context.@bool().GetText();
            var valueParsed = bool.Parse(value);
            AddOpCode(new ConstToRegisterOpCode(_expressionTerminatorRegister, new BoolValueType(valueParsed)));
        }
        else if (context.@string() != null)
        {
            var value = context.@string().STRING().GetText();
            AddOpCode(new ConstToRegisterOpCode(_expressionTerminatorRegister, new StringValueType(value)));
        }
        else if (context.methodCall() != null)
        {
            // TODO method call validity check
            throw new NotImplementedException();
        }
    }

    public override void ExitExpression(MovieScriptDefaultGrammarParser.ExpressionContext context)
    {
        /*
         * | expression (MULTIPLY | DIVIDE | MODULO) expression
         * | expression (PLUS | MINUS) expression
         * | MINUS expression
         * | NOT expression
           | expression (AND | OR) expression
           | expression (EQUAL | NOT_EQUAL | LESS | LESS_EQUAL | GREATER | GREATER_EQUAL) expression
           | expression (BITWISE_AND | BITWISE_OR | BITWISE_XOR) expression
           | expression (BITWISE_SHIFT_LEFT | BITWISE_SHIFT_RIGHT) expression
         */
    }

    public override void EnterTupleExpression(MovieScriptDefaultGrammarParser.TupleExpressionContext context)
    {
        // reserve for tuple / array creation
        if (_reserveTupleListRegister != null) return;
        _reserveTupleListRegister = AllocateTempRegister();
    }

    public override void ExitTupleExpression(MovieScriptDefaultGrammarParser.TupleExpressionContext context)
    {
        // recursively check if there's any more tuple / array expression on top level
        var parent = context.Parent;
        while (true)
        {
            if (parent is not MovieScriptDefaultGrammarParser.ProgramContext)
            {
                // ReSharper disable once PossibleInvalidOperationException
                DeallocateTempRegister(_reserveTupleListRegister.Value);
                break;
            }

            if (parent.Parent == null)
                break;
            parent = parent.Parent;
        }
    }
}