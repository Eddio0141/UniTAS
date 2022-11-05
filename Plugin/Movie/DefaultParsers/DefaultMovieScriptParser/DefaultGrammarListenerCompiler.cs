using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UniTASPlugin.Movie.Models.Script;
using UniTASPlugin.Movie.ScriptEngine;
using UniTASPlugin.Movie.ScriptEngine.OpCodes;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.BitwiseOps;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Maths;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Method;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.RegisterSet;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.VariableSet;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.DefaultParsers.DefaultMovieScriptParser;

public class DefaultGrammarListenerCompiler : MovieScriptDefaultGrammarBaseListener
{
    private class MethodBuilder
    {
        public string Name { get; }
        public List<OpCodeBase> OpCodes { get; set; } = new();
        public List<string> DeclaredVariables { get; set; } = new();

        public MethodBuilder(string name)
        {
            Name = name;
        }

        public KeyValuePair<string, List<OpCodeBase>> GetFinalResult()
        {
            return new KeyValuePair<string, List<OpCodeBase>>(Name, OpCodes);
        }
    }

    private readonly List<OpCodeBase> _mainBuilder = new();
    private readonly List<string> _mainScopeVariable = new();
    private readonly List<KeyValuePair<string, List<OpCodeBase>>> _builtMethods = new();
    private readonly Stack<MethodBuilder> _methodBuilders = new();
    private bool _buildingMethod;

    private RegisterType? _tupleListRegisterReserve;

    private RegisterType? _expressionTerminatorRegisterLeftReserve;
    private RegisterType _expressionTerminatorRegisterRightReserve;
    // reserved register for using expression on operations like variable assignment, method argument, use of expression result for another expression, etc
    private RegisterType? _expressionUseRegisterReserve;

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
            _methodBuilders.Peek().OpCodes.Add(opCode);
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
        _methodBuilders.Push(new(methodName));
    }

    public override void ExitMethodDef(MovieScriptDefaultGrammarParser.MethodDefContext context)
    {
        _builtMethods.Add(_methodBuilders.Pop().GetFinalResult());
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
        RegisterType usingRegister;
        if (_expressionTerminatorRegisterLeftReserve == null)
        {
            _expressionTerminatorRegisterLeftReserve = AllocateTempRegister();
            usingRegister = (RegisterType)_expressionTerminatorRegisterLeftReserve;
        }
        else
        {
            _expressionTerminatorRegisterRightReserve = AllocateTempRegister();
            usingRegister = _expressionTerminatorRegisterRightReserve;
        }
        if (context.variable() != null)
        {
            var variable = context.variable().IDENTIFIER_STRING().GetText();
            AddOpCode(new VarToRegisterOpCode(usingRegister, variable));
        }
        else if (context.intType() != null)
        {
            var value = context.intType().INT().GetText();
            var valueParsed = int.Parse(value);
            AddOpCode(new ConstToRegisterOpCode(usingRegister, new IntValueType(valueParsed)));
        }
        else if (context.floatType() != null)
        {
            var value = context.floatType().FLOAT().GetText();
            var valueParsed = float.Parse(value);
            AddOpCode(new ConstToRegisterOpCode(usingRegister, new FloatValueType(valueParsed)));
        }
        else if (context.@bool() != null)
        {
            var value = context.@bool().GetText();
            var valueParsed = bool.Parse(value);
            AddOpCode(new ConstToRegisterOpCode(usingRegister, new BoolValueType(valueParsed)));
        }
        else if (context.@string() != null)
        {
            var value = context.@string().STRING().GetText();
            AddOpCode(new ConstToRegisterOpCode(usingRegister, new StringValueType(value)));
        }
        else if (context.methodCall() != null)
        {
            // TODO method call validity check
            throw new NotImplementedException();
        }
    }

    public override void ExitExpression(MovieScriptDefaultGrammarParser.ExpressionContext context)
    {
        Debug.Assert(_expressionTerminatorRegisterLeftReserve != null, nameof(_expressionTerminatorRegisterLeftReserve) + " != null");
        _expressionUseRegisterReserve = _expressionTerminatorRegisterLeftReserve.Value;
        var res = _expressionUseRegisterReserve.Value;
        var left = _expressionTerminatorRegisterLeftReserve.Value;
        var right = _expressionTerminatorRegisterRightReserve;

        if (context.MULTIPLY() != null)
        {
            AddOpCode(new MultOpCode(res, left, right));
        }
        else
        if (context.DIVIDE() != null)
        {
            AddOpCode(new DivOpCode(res, left, right));
        }
        else if (context.MODULO() != null)
        {
            AddOpCode(new ModOpCode(res, left, right));
        }
        else if (context.PLUS() != null)
        {
            AddOpCode(new AddOpCode(res, left, right));
        }
        else if (context.MINUS() != null)
        {
            AddOpCode(new SubOpCode(res, left, right));
        }
        else if (context.setNegative != null)
        {
            AddOpCode(new FlipNegativeOpCode(res, left));
        }
        else if (context.NOT() != null)
        {
            AddOpCode(new NotOpCode(left));
        }
        else if (context.AND() != null)
        {
            AddOpCode(new AndOpCode(res, left, right));
        }
        else if (context.OR() != null)
        {
            AddOpCode(new OrOpCode(res, left, right));
        }
        else if (context.BITWISE_AND() != null)
        {
            AddOpCode(new BitwiseAndOpCode(res, left, right));
        }
        else if (context.BITWISE_OR() != null)
        {
            AddOpCode(new BitwiseOrOpCode(res, left, right));
        }
        else if (context.BITWISE_XOR() != null)
        {
            AddOpCode(new BitwiseXorOpCode(res, left, right));
        }
        else if (context.BITWISE_SHIFT_LEFT() != null)
        {
            AddOpCode(new BitwiseShiftLeftOpCode(res, left, right));
        }
        else if (context.BITWISE_SHIFT_RIGHT() != null)
        {
            AddOpCode(new BitwiseShiftRightOpCode(res, left, right));
        }

        _expressionTerminatorRegisterLeftReserve = null;
        DeallocateTempRegister(_expressionTerminatorRegisterRightReserve);
    }

    public override void EnterTupleExpression(MovieScriptDefaultGrammarParser.TupleExpressionContext context)
    {
        // reserve for tuple / array creation
        if (_tupleListRegisterReserve != null) return;
        _tupleListRegisterReserve = AllocateTempRegister();
    }

    public override void ExitTupleExpression(MovieScriptDefaultGrammarParser.TupleExpressionContext context)
    {
        // recursively check if there's any more tuple / array expression on top level
        var parent = context.Parent;
        while (true)
        {
            if (parent is not MovieScriptDefaultGrammarParser.ProgramContext)
            {
                Debug.Assert(_tupleListRegisterReserve != null, nameof(_tupleListRegisterReserve) + " != null");
                DeallocateTempRegister(_tupleListRegisterReserve.Value);
                break;
            }

            if (parent.Parent == null)
                break;
            parent = parent.Parent;
        }
    }

    public override void ExitVariableAssignment(MovieScriptDefaultGrammarParser.VariableAssignmentContext context)
    {
        RegisterType usingRegister;
        if (_expressionUseRegisterReserve != null)
        {
            usingRegister = _expressionUseRegisterReserve.Value;
            _expressionUseRegisterReserve = null;
        }
        else/* if (_tupleListRegisterReserve != null)*/
        {
            Debug.Assert(_tupleListRegisterReserve != null, nameof(_tupleListRegisterReserve) + " != null");
            usingRegister = _tupleListRegisterReserve.Value;
            _tupleListRegisterReserve = null;
        }

        var variableName = context.variable().IDENTIFIER_STRING().GetText();

        if (context.ASSIGN() != null)
        {
            // TODO
        }

        AddOpCode(new SetVariableOpCode(variableName, usingRegister));

        DeallocateTempRegister(usingRegister);
    }
}