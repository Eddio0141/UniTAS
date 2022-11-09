using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UniTASPlugin.Movie.Exceptions.ParseExceptions;
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
using static MovieScriptDefaultGrammarParser;

namespace UniTASPlugin.Movie.DefaultParsers.DefaultMovieScriptParser;

public class DefaultGrammarListenerCompiler : MovieScriptDefaultGrammarBaseListener
{
    private class MethodBuilder
    {
        public string Name { get; }
        public List<OpCodeBase> OpCodes { get; set; } = new();

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
    private readonly List<KeyValuePair<string, List<OpCodeBase>>> _builtMethods = new();
    private readonly Stack<MethodBuilder> _methodBuilders = new();
    private bool _buildingMethod;

    private RegisterType? _tupleListRegisterReserve;

    private readonly Stack<List<ExpressionBase>> _expressionBuilders = new();

    private readonly bool[] _reservedTempRegister = new bool[RegisterType.Temp5 - RegisterType.Temp + 1];
    private readonly Stack<List<int>> _reservedRegisterStackTrack = new();

    private RegisterType? _methodCallArgStore;
    private int _methodCallArgStoreCount;

    public IEnumerable<ScriptMethodModel> Compile()
    {
        var methods = new List<ScriptMethodModel> { new(null, _mainBuilder) };
        methods.AddRange(_builtMethods.Select(x => new ScriptMethodModel(x.Key, x.Value)));
        return methods;
    }

    private void PushUsingTempRegisters()
    {
        _reservedRegisterStackTrack.Push(new());
        for (var i = 0; i < _reservedTempRegister.Length; i++)
        {
            var @using = _reservedTempRegister[i];
            var register = RegisterType.Temp + i;
            if (@using)
            {
                AddOpCode(new PushStackOpCode(register));
                _reservedRegisterStackTrack.Peek().Add(i);
            }

            _reservedTempRegister[i] = false;
        }
    }

    private void PopUsingTempRegisters()
    {
        var usingIndexes = _reservedRegisterStackTrack.Pop();
        foreach (var usingIndex in usingIndexes)
        {
            var register = RegisterType.Temp + usingIndex;
            AddOpCode(new PopStackOpCode(register));
            _reservedTempRegister[usingIndex] = true;
        }
    }

    private bool FindDefinedMethod(string name)
    {
        return _builtMethods.Any(x => x.Key == name) || _methodBuilders.Any(x => x.Name == name);
    }

    private void AddExpression(ExpressionBase expression)
    {
        _expressionBuilders.Peek().Add(expression);
    }

    private void PushExpressionBuilderStack()
    {
        _expressionBuilders.Push(new());
    }

    private RegisterType BuildExpressionOpCodes()
    {
        var expressionBuilder = _expressionBuilders.Peek();
        var i = 0;
        OperationType? op = null;
        ExpressionBase left = null;
        var leftRegister = AllocateTempRegister();
        var rightRegister = AllocateTempRegister();
        var storeRegister = AllocateTempRegister();
        // loop until expression is const, var, method call, or evaluated
        while (true)
        {
            var expr = expressionBuilder[i];
            ExpressionBase right = null;
            switch (expr)
            {
                case OperationExpression opExpression:
                    op = opExpression.Operation;
                    if (left is EvaluatedExpression)
                    {
                        // move register to store since we are resetting left and right
                        AddOpCode(new MoveOpCode(leftRegister, storeRegister));
                    }
                    left = null;
                    i++;
                    continue;
                case ConstExpression @const when left == null:
                    left = @const;
                    i++;
                    break;
                case ConstExpression @const:
                    right = @const;
                    i++;
                    break;
                case VariableExpression var when left == null:
                    left = var;
                    i++;
                    break;
                case VariableExpression var:
                    right = var;
                    i++;
                    break;
                case EvaluatedExpression evaluated when left == null:
                    left = evaluated;
                    i++;
                    break;
                case EvaluatedExpression evaluated:
                    right = evaluated;
                    i++;
                    break;
                case MethodCallExpression methodCall:

                default:
                    throw new NotImplementedException();
            }

            if (op == null)
            {
                break;
            }

            // operations that only takes a left to evaluate
            if (op.Value != OperationType.FlipNegative && op.Value != OperationType.Not && right == null)
            {
                continue;
            }

            switch (left)
            {
                case ConstExpression leftConst:
                    AddOpCode(new ConstToRegisterOpCode(leftRegister, leftConst.Value));
                    break;
                case VariableExpression var:
                    AddOpCode(new VarToRegisterOpCode(leftRegister, var.Name));
                    break;
            }
            switch (right)
            {
                case ConstExpression rightConst:
                    AddOpCode(new ConstToRegisterOpCode(rightRegister, rightConst.Value));
                    break;
                case VariableExpression var:
                    AddOpCode(new VarToRegisterOpCode(rightRegister, var.Name));
                    break;
            }

            var usingLeft = left is EvaluatedExpression ? storeRegister : leftRegister;
            var usingRight = right is EvaluatedExpression ? storeRegister : rightRegister;

            switch (op.Value)
            {
                case OperationType.FlipNegative:
                    AddOpCode(new FlipNegativeOpCode(usingLeft, leftRegister));
                    break;
                case OperationType.Mult:
                    AddOpCode(new MultOpCode(leftRegister, usingLeft, usingRight));
                    break;
                case OperationType.Div:
                    AddOpCode(new DivOpCode(leftRegister, usingLeft, usingRight));
                    break;
                case OperationType.Mod:
                    AddOpCode(new ModOpCode(leftRegister, usingLeft, usingRight));
                    break;
                case OperationType.Add:
                    AddOpCode(new AddOpCode(leftRegister, usingLeft, usingRight));
                    break;
                case OperationType.Subtract:
                    AddOpCode(new SubOpCode(leftRegister, usingLeft, usingRight));
                    break;
                case OperationType.Not:
                    AddOpCode(new NotOpCode(leftRegister, usingLeft));
                    break;
                case OperationType.AndLogic:
                    AddOpCode(new AndOpCode(leftRegister, usingLeft, usingRight));
                    break;
                case OperationType.OrLogic:
                    AddOpCode(new OrOpCode(leftRegister, usingLeft, usingRight));
                    break;
                case OperationType.EqualsLogic:
                    AddOpCode(new EqualOpCode(leftRegister, usingLeft, usingRight));
                    break;
                case OperationType.NotEqualsLogic:
                    AddOpCode(new NotEqualOpCode(leftRegister, usingLeft, usingRight));
                    break;
                case OperationType.LessLogic:
                    AddOpCode(new LessOpCode(leftRegister, usingLeft, usingRight));
                    break;
                case OperationType.LessEqualsLogic:
                    AddOpCode(new LessEqualOpCode(leftRegister, usingLeft, usingRight));
                    break;
                case OperationType.GreaterLogic:
                    AddOpCode(new GreaterOpCode(leftRegister, usingLeft, usingRight));
                    break;
                case OperationType.GreaterEqualsLogic:
                    AddOpCode(new GreaterEqualOpCode(leftRegister, usingLeft, usingRight));
                    break;
                case OperationType.BitwiseAnd:
                    AddOpCode(new BitwiseAndOpCode(leftRegister, usingLeft, usingRight));
                    break;
                case OperationType.BitwiseOr:
                    AddOpCode(new BitwiseOrOpCode(leftRegister, usingLeft, usingRight));
                    break;
                case OperationType.BitwiseXor:
                    AddOpCode(new BitwiseXorOpCode(leftRegister, usingLeft, usingRight));
                    break;
                case OperationType.BitwiseShiftLeft:
                    AddOpCode(new BitwiseShiftLeftOpCode(leftRegister, usingLeft, usingRight));
                    break;
                case OperationType.BitwiseShiftRight:
                    AddOpCode(new BitwiseShiftRightOpCode(leftRegister, usingLeft, usingRight));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // op is done so we remove stuff
            op = null;
            expressionBuilder.RemoveAt(i - 1);
            expressionBuilder.RemoveAt(i - 2);
            var insertIndex = i - 2;
            i -= 3;
            if (right != null)
            {
                expressionBuilder.RemoveAt(i);
                insertIndex--;
                i--;
            }
            expressionBuilder.Insert(insertIndex, new EvaluatedExpression());
            left = null;

            // wind back to next op
            while (i > -1)
            {
                var exprPrev = expressionBuilder[i];
                if (exprPrev is OperationExpression)
                {
                    break;
                }
                i--;
            }

            if (i < 0)
                break;
        }

        // we ran out of expressions to process, finish up
        Debug.Assert(_expressionBuilders.Count == 1, "There should be only a single evaluated expression, or a const, or some method call left");
        switch (expressionBuilder[0])
        {
            case ConstExpression constExpression:
                AddOpCode(new ConstToRegisterOpCode(leftRegister, constExpression.Value));
                break;
            /*
            case EvaluatedExpression evaluatedExpression:
                break;
            */
            case VariableExpression variableExpression:
                AddOpCode(new VarToRegisterOpCode(leftRegister, variableExpression.Name));
                break;
        }

        expressionBuilder.Clear();
        DeallocateTempRegister(rightRegister);
        DeallocateTempRegister(storeRegister);

        return leftRegister;
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
            var methodName = context.methodCall().IDENTIFIER_STRING().GetText();
            if (!FindDefinedMethod(methodName))
            {
                throw new UsingUndefinedMethodException(methodName);
            }
            AddExpression(new MethodCallExpression(methodName));
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

    public override void EnterVariableAssignment(VariableAssignmentContext context)
    {
        PushExpressionBuilderStack();
    }

    public override void ExitVariableAssignment(VariableAssignmentContext context)
    {
        var usingRegister = BuildExpressionOpCodes();
        var variableName = context.variable().IDENTIFIER_STRING().GetText();

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

    public override void EnterMethodCall(MethodCallContext context)
    {
        if (context.methodCallArgs() == null) return;
        PushExpressionBuilderStack();
        if (context.Parent is ExpressionContext)
        {
            PushUsingTempRegisters();
        }
    }

    public override void ExitMethodCall(MethodCallContext context)
    {
        for (var i = 0; i < _methodCallArgStoreCount; i++)
        {
            
        }
        if (context.Parent is ExpressionContext)
        {
            PopUsingTempRegisters();
        }
    }

    public override void ExitMethodCallArgs(MethodCallArgsContext context)
    {
        var usingRegister = BuildExpressionOpCodes();
        // HACK
        if (_methodCallArgStore == null)
        {
            _methodCallArgStore = usingRegister;
        }
        else
        {
            AddOpCode(new PushStackOpCode(_methodCallArgStore.Value));
        }
        _methodCallArgStoreCount++;
    }
}