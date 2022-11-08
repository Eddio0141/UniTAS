using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using UniTASPlugin.Movie.Models.Script;
using UniTASPlugin.Movie.ScriptEngine;
using UniTASPlugin.Movie.ScriptEngine.OpCodes;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Maths;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Method;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.RegisterSet;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;
using static MovieScriptDefaultGrammarParser;

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

    private readonly List<ExpressionBase> _expressionBuilder = new();

    private readonly bool[] _reservedTempRegister = new bool[RegisterType.Temp5 - RegisterType.Temp + 1];

    public IEnumerable<ScriptMethodModel> Compile()
    {
        var methods = new List<ScriptMethodModel>();
        methods.Add(new(null, _mainBuilder));
        methods.AddRange(_builtMethods.Select(x => new ScriptMethodModel(x.Key, x.Value)));
        return methods;
    }

    private void AddExpression(ExpressionBase expression)
    {
        _expressionBuilder.Add(expression);
    }

    private KeyValuePair<IEnumerable<OpCodeBase>, RegisterType> BuildExpressionOpCodes()
    {
        var i = 0;
        OperationType? op = null;
        ExpressionBase left = null;
        ExpressionBase right = null;
        var leftRegister = AllocateTempRegister();
        var rightRegister = AllocateTempRegister();
        var storeRegister = AllocateTempRegister();
        while (i < _expressionBuilder.Count)
        {
            var expr = _expressionBuilder[i];
            i++;
            if (expr is OperationExpression opExpression)
            {
                op = opExpression.Operation;
                left = null;
                right = null;
                continue;
            }

            if (expr is ConstExpression @const)
            {
                if (left == null)
                {
                    left = @const;
                }
                else
                {
                    right = @const;
                }
            }
            else if (expr is VariableExpression var)
            {
                if (left == null)
                {
                    left = var;
                }
                else
                {
                    right = var;
                }
            }

            Debug.Assert(op != null, nameof(op) + " != null");
            switch (op.Value)
            {
                case OperationType.FlipNegative:
                    break;
                case OperationType.Mult:
                    break;
                case OperationType.Div:
                    break;
                case OperationType.Mod:
                    break;
                case OperationType.Add:
                    if (right == null)
                    {
                        continue;
                    }
                    // TODO consider about storeRegister
                    AddOpCode(new AddOpCode(leftRegister, leftRegister, rightRegister));
                    break;
                case OperationType.Subtract:
                    break;
                case OperationType.Not:
                    break;
                case OperationType.AndLogic:
                    break;
                case OperationType.OrLogic:
                    break;
                case OperationType.EqualsLogic:
                    break;
                case OperationType.NotEqualsLogic:
                    break;
                case OperationType.LessLogic:
                    break;
                case OperationType.LessEqualsLogic:
                    break;
                case OperationType.GreaterLogic:
                    break;
                case OperationType.GreaterEqualsLogic:
                    break;
                case OperationType.BitwiseAnd:
                    break;
                case OperationType.BitwiseOr:
                    break;
                case OperationType.BitwiseXor:
                    break;
                case OperationType.BitwiseShiftLeft:
                    break;
                case OperationType.BitwiseShiftRight:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        throw new NotImplementedException();
        //return new KeyValuePair<IEnumerable<OpCodeBase>, RegisterType>(opCodes, );
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

    private void AddOpCodes(IEnumerable<OpCodeBase> opCodes)
    {
        if (_buildingMethod)
        {
            _methodBuilders.Peek().OpCodes.AddRange(opCodes);
        }
        else
        {
            _mainBuilder.AddRange(opCodes);
        }
    }

    public override void EnterMethodDef(MethodDefContext context)
    {
        var methodName = context.IDENTIFIER_STRING().GetText();
        _buildingMethod = true;
        _methodBuilders.Push(new(methodName));
    }

    public override void ExitMethodDef(MethodDefContext context)
    {
        _builtMethods.Add(_methodBuilders.Pop().GetFinalResult());
        if (_methodBuilders.Count == 0)
        {
            _buildingMethod = false;
        }
    }

    public override void ExitMethodDefArgs(MethodDefArgsContext context)
    {
        var argName = context.IDENTIFIER_STRING().GetText();
        AddOpCode(new PopArgOpCode(RegisterType.Temp));
        AddOpCode(new SetVariableOpCode(RegisterType.Temp, argName));
    }

    public override void EnterFlipSign(FlipSignContext context)
    {
        AddExpression(new OperationExpression(OperationType.FlipNegative));
    }

    public override void EnterMultiplyDivide(MultiplyDivideContext context)
    {
        OperationType opType;
        if (context.MULTIPLY() != null)
        {
            opType = OperationType.Mult;
        }
        else if (context.DIVIDE() != null)
        {
            opType = OperationType.Div;
        }
        else if (context.MODULO() != null)
        {
            opType = OperationType.Mod;
        }
        else
        {
            throw new InvalidOperationException();
        }
        AddExpression(new OperationExpression(opType));
    }

    public override void EnterAddSubtract(AddSubtractContext context)
    {
        OperationType opType;
        if (context.PLUS() != null)
        {
            opType = OperationType.Add;
        }
        else if (context.MINUS() != null)
        {
            opType = OperationType.Subtract;
        }
        else
        {
            throw new InvalidOperationException();
        }
        AddExpression(new OperationExpression(opType));
    }

    public override void EnterNot(NotContext context)
    {
        AddExpression(new OperationExpression(OperationType.Not));
    }

    public override void EnterAndOr(AndOrContext context)
    {
        OperationType opType;
        if (context.AND() != null)
        {
            opType = OperationType.AndLogic;
        }
        else if (context.OR() != null)
        {
            opType = OperationType.OrLogic;
        }
        else
        {
            throw new InvalidOperationException();
        }
        AddExpression(new OperationExpression(opType));
    }

    public override void EnterCompare(CompareContext context)
    {
        OperationType opType;
        if (context.EQUAL() != null)
        {
            opType = OperationType.EqualsLogic;
        }
        else if (context.NOT_EQUAL() != null)
        {
            opType = OperationType.NotEqualsLogic;
        }
        else if (context.LESS() != null)
        {
            opType = OperationType.LessLogic;
        }
        else if (context.LESS_EQUAL() != null)
        {
            opType = OperationType.LessEqualsLogic;
        }
        else if (context.GREATER() != null)
        {
            opType = OperationType.GreaterLogic;
        }
        else if (context.GREATER_EQUAL() != null)
        {
            opType = OperationType.GreaterEqualsLogic;
        }
        else
        {
            throw new InvalidOperationException();
        }
        AddExpression(new OperationExpression(opType));
    }

    public override void EnterBitwise(BitwiseContext context)
    {
        OperationType opType;
        if (context.BITWISE_AND() != null)
        {
            opType = OperationType.BitwiseAnd;
        }
        else if (context.BITWISE_OR() != null)
        {
            opType = OperationType.BitwiseOr;
        }
        else if (context.BITWISE_XOR() != null)
        {
            opType = OperationType.BitwiseXor;
        }
        else
        {
            throw new InvalidOperationException();
        }
        AddExpression(new OperationExpression(opType));
    }

    public override void EnterBitwiseShift(BitwiseShiftContext context)
    {
        OperationType opType;
        if (context.BITWISE_SHIFT_LEFT() != null)
        {
            opType = OperationType.BitwiseShiftLeft;
        }
        else if (context.BITWISE_SHIFT_RIGHT() != null)
        {
            opType = OperationType.BitwiseShiftRight;
        }
        else
        {
            throw new InvalidOperationException();
        }
        AddExpression(new OperationExpression(opType));
    }

    public override void ExitTerminator(TerminatorContext context)
    {
        if (context.variable() != null)
        {
            var variable = context.variable().IDENTIFIER_STRING().GetText();
            AddExpression(new VariableExpression(variable));
        }
        else if (context.intType() != null)
        {
            var value = context.intType().INT().GetText();
            var valueParsed = int.Parse(value);
            AddExpression(new ConstExpression(new IntValueType(valueParsed)));
        }
        else if (context.floatType() != null)
        {
            var value = context.floatType().FLOAT().GetText();
            var valueParsed = float.Parse(value);
            AddExpression(new ConstExpression(new FloatValueType(valueParsed)));
        }
        else if (context.@bool() != null)
        {
            var value = context.@bool().GetText();
            var valueParsed = bool.Parse(value);
            AddExpression(new ConstExpression(new BoolValueType(valueParsed)));
        }
        else if (context.@string() != null)
        {
            var value = context.@string().STRING().GetText();
            AddExpression(new ConstExpression(new StringValueType(value)));
        }
        else if (context.methodCall() != null)
        {
            // TODO method call validity check
            throw new NotImplementedException();
        }
    }

    public override void EnterTupleExpression(TupleExpressionContext context)
    {
        // reserve for tuple / array creation TODO
        //if (_tupleListRegisterReserve != null) return;
        //_tupleListRegisterReserve = AllocateTempRegister();
    }

    public override void ExitTupleExpression(TupleExpressionContext context)
    {
        // recursively check if there's any more tuple / array expression on top level
        var parent = context.Parent;
        while (true)
        {
            if (parent is not ProgramContext)
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

    public override void ExitVariableAssignment(VariableAssignmentContext context)
    {
        var expressionBuildResult = BuildExpressionOpCodes();
        AddOpCodes(expressionBuildResult.Key);

        var variableName = context.variable().IDENTIFIER_STRING().GetText();
        var usingRegister = expressionBuildResult.Value;

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

    public override void EnterFrameAdvance(FrameAdvanceContext context)
    {
        AddOpCode(new FrameAdvanceOpCode());
    }
}