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
using UniTASPlugin.Movie.ScriptEngine.OpCodes.StackOp;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.DefaultParsers.DefaultMovieScriptParser;

public class DefaultGrammarListenerCompiler : MovieScriptDefaultGrammarBaseListener
{
    private class MethodBuilder
    {
        private readonly string _name;
        public List<OpCodeBase> OpCodes { get; set; } = new();

        public MethodBuilder(string name)
        {
            _name = name;
        }

        public KeyValuePair<string, List<OpCodeBase>> GetFinalResult()
        {
            return new KeyValuePair<string, List<OpCodeBase>>(_name, OpCodes);
        }
    }

    private readonly List<OpCodeBase> _mainBuilder = new();
    private readonly List<KeyValuePair<string, List<OpCodeBase>>> _builtMethods = new();
    private readonly Stack<MethodBuilder> _methodBuilders = new();
    private bool _buildingMethod;

    private RegisterType? _tupleListRegisterReserve;

    private RegisterType? _expressionTerminatorRegisterLeftReserve;
    private RegisterType? _expressionTerminatorRegisterRightReserve;
    private RegisterType? _bracketExpressionStoreReserve;
    private int _bracketExpressionStoreReserveStack = -1;

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
        AddOpCode(new SetVariableOpCode(RegisterType.Temp, argName));
    }

    public override void ExitExpressionTerminator(MovieScriptDefaultGrammarParser.ExpressionTerminatorContext context)
    {
        RegisterType usingRegister;
        if (_expressionTerminatorRegisterLeftReserve == null)
        {
            _expressionTerminatorRegisterLeftReserve = AllocateTempRegister();
            usingRegister = _expressionTerminatorRegisterLeftReserve.Value;
        }
        else
        {
            _expressionTerminatorRegisterRightReserve = AllocateTempRegister();
            usingRegister = _expressionTerminatorRegisterRightReserve.Value;
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

    private void BracketExpressionStoreReserveSetLeft()
    {
        Debug.Assert(_bracketExpressionStoreReserve != null, nameof(_bracketExpressionStoreReserve) + " != null");
        if (_expressionTerminatorRegisterLeftReserve == null)
        {
            _expressionTerminatorRegisterLeftReserve = AllocateTempRegister();
            AddOpCode(new MoveOpCode(_bracketExpressionStoreReserve.Value, _expressionTerminatorRegisterLeftReserve.Value));
        }
        else
        {
            _expressionTerminatorRegisterRightReserve = AllocateTempRegister();
            AddOpCode(new MoveOpCode(_bracketExpressionStoreReserve.Value, _expressionTerminatorRegisterRightReserve.Value));
        }

        _bracketExpressionStoreReserveStack--;
        if (_bracketExpressionStoreReserveStack < 0)
        {
            DeallocateTempRegister(_bracketExpressionStoreReserve.Value);
            _bracketExpressionStoreReserve = null;
        }
        else
        {
            AddOpCode(new PopStackOpCode(_bracketExpressionStoreReserve.Value));
        }
    }

    public override void ExitExpression(MovieScriptDefaultGrammarParser.ExpressionContext context)
    {
        if (context.expressionTerminator() != null)
            return;
        if (context.ROUND_BRACKET_OPEN() != null)
        {
            if (_bracketExpressionStoreReserve == null)
            {
                _bracketExpressionStoreReserve = AllocateTempRegister();
            }
            else
            {
                AddOpCode(new PushStackOpCode(_bracketExpressionStoreReserve.Value));
            }
            _bracketExpressionStoreReserveStack++;

            Debug.Assert(_expressionTerminatorRegisterLeftReserve != null, nameof(_expressionTerminatorRegisterLeftReserve) + " != null");
            AddOpCode(new MoveOpCode(_expressionTerminatorRegisterLeftReserve.Value, _bracketExpressionStoreReserve.Value));
            DeallocateTempRegister(_expressionTerminatorRegisterLeftReserve.Value);
            _expressionTerminatorRegisterLeftReserve = null;
            return;
        }

        if (context.GetChild(0) is MovieScriptDefaultGrammarParser.ExpressionContext childContext && childContext.ROUND_BRACKET_OPEN() != null)
        {
            BracketExpressionStoreReserveSetLeft();
        }

        if (_expressionTerminatorRegisterLeftReserve == null && _bracketExpressionStoreReserve != null)
        {
            BracketExpressionStoreReserveSetLeft();
        }
        else if (_expressionTerminatorRegisterLeftReserve == null && _expressionTerminatorRegisterRightReserve == null)
        {
            throw new InvalidOperationException(
                "Left and Right reserved registers are both null, means the (expr) made both Left and Right null, then there's no exprTerminator token next so Left is never set again");
        }

        Debug.Assert(_expressionTerminatorRegisterLeftReserve != null, nameof(_expressionTerminatorRegisterLeftReserve) + " != null");
        var res = _expressionTerminatorRegisterLeftReserve.Value;
        var left = _expressionTerminatorRegisterLeftReserve.Value;
        var right = _expressionTerminatorRegisterRightReserve;

        if (context.MULTIPLY() != null)
        {
            Debug.Assert(right != null, nameof(right) + " != null");
            AddOpCode(new MultOpCode(res, left, right.Value));
        }
        else
        if (context.DIVIDE() != null)
        {
            Debug.Assert(right != null, nameof(right) + " != null");
            AddOpCode(new DivOpCode(res, left, right.Value));
        }
        else if (context.MODULO() != null)
        {
            Debug.Assert(right != null, nameof(right) + " != null");
            AddOpCode(new ModOpCode(res, left, right.Value));
        }
        else if (context.PLUS() != null)
        {
            Debug.Assert(right != null, nameof(right) + " != null");
            AddOpCode(new AddOpCode(res, left, right.Value));
        }
        else if (context.subtract != null)
        {
            Debug.Assert(right != null, nameof(right) + " != null");
            AddOpCode(new SubOpCode(res, left, right.Value));
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
            Debug.Assert(right != null, nameof(right) + " != null");
            AddOpCode(new AndOpCode(res, left, right.Value));
        }
        else if (context.OR() != null)
        {
            Debug.Assert(right != null, nameof(right) + " != null");
            AddOpCode(new OrOpCode(res, left, right.Value));
        }
        else if (context.BITWISE_AND() != null)
        {
            Debug.Assert(right != null, nameof(right) + " != null");
            AddOpCode(new BitwiseAndOpCode(res, left, right.Value));
        }
        else if (context.BITWISE_OR() != null)
        {
            Debug.Assert(right != null, nameof(right) + " != null");
            AddOpCode(new BitwiseOrOpCode(res, left, right.Value));
        }
        else if (context.BITWISE_XOR() != null)
        {
            Debug.Assert(right != null, nameof(right) + " != null");
            AddOpCode(new BitwiseXorOpCode(res, left, right.Value));
        }
        else if (context.BITWISE_SHIFT_LEFT() != null)
        {
            Debug.Assert(right != null, nameof(right) + " != null");
            AddOpCode(new BitwiseShiftLeftOpCode(res, left, right.Value));
        }
        else if (context.BITWISE_SHIFT_RIGHT() != null)
        {
            Debug.Assert(right != null, nameof(right) + " != null");
            AddOpCode(new BitwiseShiftRightOpCode(res, left, right.Value));
        }

        if (_expressionTerminatorRegisterRightReserve == null) return;
        DeallocateTempRegister(_expressionTerminatorRegisterRightReserve.Value);
        _expressionTerminatorRegisterRightReserve = null;
    }

    public override void EnterTupleExpression(MovieScriptDefaultGrammarParser.TupleExpressionContext context)
    {
        // reserve for tuple / array creation TODO
        //if (_tupleListRegisterReserve != null) return;
        //_tupleListRegisterReserve = AllocateTempRegister();
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
        var variableName = context.variable().IDENTIFIER_STRING().GetText();
        Debug.Assert(_expressionTerminatorRegisterLeftReserve != null, nameof(_expressionTerminatorRegisterLeftReserve) + " != null");
        var usingRegister = _expressionTerminatorRegisterLeftReserve.Value;
        _expressionTerminatorRegisterLeftReserve = null;

        if (context.ASSIGN() == null)
        {
            var reserveAdd = AllocateTempRegister();
            AddOpCode(new VarToRegisterOpCode(reserveAdd, variableName));

            // +
            if (context.PLUS_ASSIGN() != null)
            {
                AddOpCode(new AddOpCode(usingRegister, reserveAdd, usingRegister));
            }
            // -
            else if (context.MINUS_ASSIGN() != null)
            {
                AddOpCode(new SubOpCode(usingRegister, reserveAdd, usingRegister));
            }
            // *
            else if (context.MULTIPLY_ASSIGN() != null)
            {
                AddOpCode(new MultOpCode(usingRegister, reserveAdd, usingRegister));
            }
            // /
            else if (context.DIVIDE_ASSIGN() != null)
            {
                AddOpCode(new DivOpCode(usingRegister, reserveAdd, usingRegister));
            }
            // %
            else if (context.MODULO_ASSIGN() != null)
            {
                AddOpCode(new ModOpCode(usingRegister, reserveAdd, usingRegister));
            }

            DeallocateTempRegister(reserveAdd);
        }

        AddOpCode(new SetVariableOpCode(usingRegister, variableName));

        DeallocateTempRegister(usingRegister);
    }

    public override void EnterFrameAdvance(MovieScriptDefaultGrammarParser.FrameAdvanceContext context)
    {
        AddOpCode(new FrameAdvanceOpCode());
    }
}