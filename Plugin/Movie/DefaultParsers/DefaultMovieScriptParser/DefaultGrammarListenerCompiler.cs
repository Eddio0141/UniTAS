using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Antlr4.Runtime;
using UniTASPlugin.Movie.DefaultParsers.DefaultMovieScriptParser.Expressions;
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
    // TODO method call validation
    // TODO addition type validation
    private class MethodBuilder
    {
        public string Name { get; }
        public List<OpCodeBase> OpCodes { get; } = new();

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
    private bool _buildingMethodCallArgs;
    // opCodes that will be ran in order to call the method
    private readonly List<OpCodeBase> _methodCallArgsBuilder = new();

    private readonly Stack<List<ExpressionBase>> _expressionBuilders = new();

    private readonly bool[] _reservedTempRegister = new bool[RegisterType.Temp9 - RegisterType.Temp + 1];
    private readonly Stack<List<int>> _reservedRegisterStackTrack = new();

    private RegisterType? _methodCallArgStore;
    private int _methodCallArgStoreCount;

    private int _tupleExprDepth;
    private RegisterType? _tupleExprTopLevelStore;
    private RegisterType? _tupleExprInnerStore;
    private readonly List<int> _tupleInnerStorePushDepths = new();

    public IEnumerable<ScriptMethodModel> Compile()
    {
        Debug.Assert(!_reservedTempRegister.Any(x => x), "Reserved temporary register is still being used, means something forgot to deallocate it");
        Debug.Assert(_expressionBuilders.Count == 0,
            "Expression builder stack should be empty, something forgot to use it or we allocated too much stack");
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
        var expressionBuilder = _expressionBuilders.Pop();
        var i = 0;
        OperationType? op = null;
        ExpressionBase left = null;
        var leftRegister = AllocateTempRegister();
        var rightRegister = AllocateTempRegister();
        var storeRegister = AllocateTempRegister();
        var useStoreRegister = false;
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
                case MethodCallExpression methodCall when left == null:
                    left = methodCall;
                    i++;
                    break;
                case MethodCallExpression methodCall:
                    right = methodCall;
                    i++;
                    break;
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
                case MethodCallExpression methodCall:
                    CallMethod(methodCall.MethodName);
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
                case MethodCallExpression methodCall:
                    CallMethod(methodCall.MethodName);
                    break;
            }

            RegisterType usingLeft;
            if (useStoreRegister)
                usingLeft = storeRegister;
            else if (left is MethodCallExpression)
                usingLeft = RegisterType.Ret;
            else
                usingLeft = leftRegister;

            var usingRight = right is MethodCallExpression ? RegisterType.Ret : rightRegister;

            var usingResult = leftRegister;
            // result will be leftRegister unless expr before this op isn't an op
            var prevExprIndex = right == null ? i - 3 : i - 4;
            if (prevExprIndex > -1 && expressionBuilder[prevExprIndex] is not OperationExpression)
            {
                usingResult = rightRegister;
            }

            switch (op.Value)
            {
                case OperationType.FlipNegative:
                    AddOpCode(new FlipNegativeOpCode(usingLeft, usingResult));
                    break;
                case OperationType.Mult:
                    AddOpCode(new MultOpCode(usingResult, usingLeft, usingRight));
                    break;
                case OperationType.Div:
                    AddOpCode(new DivOpCode(usingResult, usingLeft, usingRight));
                    break;
                case OperationType.Mod:
                    AddOpCode(new ModOpCode(usingResult, usingLeft, usingRight));
                    break;
                case OperationType.Add:
                    AddOpCode(new AddOpCode(usingResult, usingLeft, usingRight));
                    break;
                case OperationType.Subtract:
                    AddOpCode(new SubOpCode(usingResult, usingLeft, usingRight));
                    break;
                case OperationType.Not:
                    AddOpCode(new NotOpCode(usingResult, usingLeft));
                    break;
                case OperationType.AndLogic:
                    AddOpCode(new AndOpCode(usingResult, usingLeft, usingRight));
                    break;
                case OperationType.OrLogic:
                    AddOpCode(new OrOpCode(usingResult, usingLeft, usingRight));
                    break;
                case OperationType.EqualsLogic:
                    AddOpCode(new EqualOpCode(usingResult, usingLeft, usingRight));
                    break;
                case OperationType.NotEqualsLogic:
                    AddOpCode(new NotEqualOpCode(usingResult, usingLeft, usingRight));
                    break;
                case OperationType.LessLogic:
                    AddOpCode(new LessOpCode(usingResult, usingLeft, usingRight));
                    break;
                case OperationType.LessEqualsLogic:
                    AddOpCode(new LessEqualOpCode(usingResult, usingLeft, usingRight));
                    break;
                case OperationType.GreaterLogic:
                    AddOpCode(new GreaterOpCode(usingResult, usingLeft, usingRight));
                    break;
                case OperationType.GreaterEqualsLogic:
                    AddOpCode(new GreaterEqualOpCode(usingResult, usingLeft, usingRight));
                    break;
                case OperationType.BitwiseAnd:
                    AddOpCode(new BitwiseAndOpCode(usingResult, usingLeft, usingRight));
                    break;
                case OperationType.BitwiseOr:
                    AddOpCode(new BitwiseOrOpCode(usingResult, usingLeft, usingRight));
                    break;
                case OperationType.BitwiseXor:
                    AddOpCode(new BitwiseXorOpCode(usingResult, usingLeft, usingRight));
                    break;
                case OperationType.BitwiseShiftLeft:
                    AddOpCode(new BitwiseShiftLeftOpCode(usingResult, usingLeft, usingRight));
                    break;
                case OperationType.BitwiseShiftRight:
                    AddOpCode(new BitwiseShiftRightOpCode(usingResult, usingLeft, usingRight));
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
            useStoreRegister = false;

            // wind back to next op
            while (i > -1)
            {
                var exprPrev = expressionBuilder[i];
                if (exprPrev is OperationExpression)
                {
                    break;
                }
                if (exprPrev is EvaluatedExpression)
                {
                    useStoreRegister = true;
                }
                i--;
            }

            if (i < 0)
                break;
        }

        // we ran out of expressions to process, finish up
        Debug.Assert(expressionBuilder.Count == 1, "There should be only a single evaluated expression, or a const, or some method call left");
        var resultRegister = leftRegister;
        switch (expressionBuilder[0])
        {
            case ConstExpression constExpression:
                AddOpCode(new ConstToRegisterOpCode(leftRegister, constExpression.Value));
                break;
            case VariableExpression variableExpression:
                AddOpCode(new VarToRegisterOpCode(leftRegister, variableExpression.Name));
                break;
            case MethodCallExpression methodCallExpression:
                CallMethod(methodCallExpression.MethodName);
                resultRegister = RegisterType.Ret;
                DeallocateTempRegister(leftRegister);
                break;
        }

        DeallocateTempRegister(rightRegister);
        DeallocateTempRegister(storeRegister);

        return resultRegister;
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
        if (register is > RegisterType.Temp9 or < RegisterType.Temp)
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
        else if (_buildingMethodCallArgs)
        {
            _methodCallArgsBuilder.Add(opCode);
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
        else if (_buildingMethodCallArgs)
        {
            _methodCallArgsBuilder.AddRange(opCodes);
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
            var methodName = context.methodCall().IDENTIFIER_STRING().GetText();
            if (!FindDefinedMethod(methodName))
            {
                throw new UsingUndefinedMethodException(methodName);
            }
            AddExpression(new MethodCallExpression(methodName));
        }
        ExitExpression(context);
    }

    public override void ExitActionWithSeparator(ActionWithSeparatorContext context)
    {
        if (context.methodCall() == null) return;
        var methodName = context.methodCall().IDENTIFIER_STRING().GetText();
        CallMethod(methodName);
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
        if (usingRegister != RegisterType.Ret)
        {
            DeallocateTempRegister(usingRegister);
        }
    }

    public override void EnterFrameAdvance(FrameAdvanceContext context)
    {
        AddOpCode(new FrameAdvanceOpCode());
    }

    public override void EnterMethodCall(MethodCallContext context)
    {
        _buildingMethodCallArgs = true;
    }

    private void CallMethod(string methodName)
    {
        AddOpCodes(_methodCallArgsBuilder);
        _methodCallArgsBuilder.Clear();
        PushUsingTempRegisters();
        AddOpCode(new GotoMethodOpCode(methodName));
        PopUsingTempRegisters();
    }

    public override void ExitMethodCall(MethodCallContext context)
    {
        var tempMoveRegister = AllocateTempRegister();
        Debug.Assert(_methodCallArgStore != null, nameof(_methodCallArgStore) + " != null");
        var methodCallArgStore = _methodCallArgStore.Value;
        for (var i = 0; i < _methodCallArgStoreCount; i++)
        {
            AddOpCode(new MoveOpCode(methodCallArgStore, tempMoveRegister));

            // pop if not last index
            if (i + 1 == _methodCallArgStoreCount) continue;
            AddOpCode(new PopStackOpCode(methodCallArgStore));
            AddOpCode(new PushStackOpCode(tempMoveRegister));
        }
        DeallocateTempRegister(methodCallArgStore);
        _methodCallArgStore = null;
        for (var i = 0; i < _methodCallArgStoreCount; i++)
        {
            AddOpCode(new PushArgOpCode(tempMoveRegister));

            // if not last index
            if (i + 1 == _methodCallArgStoreCount) continue;
            AddOpCode(new PopStackOpCode(tempMoveRegister));
        }

        _methodCallArgStoreCount = 0;
        DeallocateTempRegister(tempMoveRegister);
        _buildingMethodCallArgs = false;
    }

    public override void EnterMethodCallArgs(MethodCallArgsContext context)
    {
        PushExpressionBuilderStack();
    }

    public override void ExitMethodCallArgs(MethodCallArgsContext context)
    {
        var usingRegister = BuildExpressionOpCodes();
        // HACK, we push the arguments to a temporary register where we unstack and push into arg stack later
        if (_methodCallArgStore == null)
        {
            if (usingRegister == RegisterType.Ret)
            {
                _methodCallArgStore = AllocateTempRegister();
                AddOpCode(new MoveOpCode(usingRegister, _methodCallArgStore.Value));
            }
            else
            {
                _methodCallArgStore = usingRegister;
            }
        }
        else
        {
            var methodCallArgStore = _methodCallArgStore.Value;
            AddOpCode(new PushStackOpCode(methodCallArgStore));
            AddOpCode(new MoveOpCode(usingRegister, methodCallArgStore));
            DeallocateTempRegister(usingRegister);
        }
        _methodCallArgStoreCount++;
    }

    public override void ExitFlipSign(FlipSignContext context)
    {
        ExitExpression(context);
    }

    public override void ExitMultiplyDivide(MultiplyDivideContext context)
    {
        ExitExpression(context);
    }

    public override void ExitAddSubtract(AddSubtractContext context)
    {
        ExitExpression(context);
    }

    public override void ExitNot(NotContext context)
    {
        ExitExpression(context);
    }

    public override void ExitAndOr(AndOrContext context)
    {
        ExitExpression(context);
    }

    public override void ExitCompare(CompareContext context)
    {
        ExitExpression(context);
    }

    public override void ExitBitwise(BitwiseContext context)
    {
        ExitExpression(context);
    }

    public override void ExitBitwiseShift(BitwiseShiftContext context)
    {
        ExitExpression(context);
    }

    public override void ExitParentheses(ParenthesesContext context)
    {
        ExitExpression(context);
    }

    private void ExitExpression(RuleContext context)
    {
        // only operate tuple stuff when this expression is evaluated
        if (context.Parent is not TupleExpressionContext) return;

        var resultRegister = BuildExpressionOpCodes();

        // if top level
        if (_tupleExprDepth == 1)
        {
            // we use expr result register as top level directly if top level is null
            if (_tupleExprTopLevelStore == null)
            {
                _tupleExprTopLevelStore = resultRegister;
            }
            // we push expr result register on top level if top level isn't null
            else
            {
                AddOpCode(new PushTupleOpCode(_tupleExprTopLevelStore.Value, resultRegister));
                DeallocateTempRegister(resultRegister);
            }

            return;
        }

        // we use expr result register as builder directly if builder is null
        // we push expr result register on builder if builder isn't null
        if (_tupleExprInnerStore == null)
        {
            _tupleExprInnerStore = resultRegister;
        }
        else
        {
            AddOpCode(new PushTupleOpCode(_tupleExprInnerStore.Value, resultRegister));
            DeallocateTempRegister(resultRegister);
        }
    }

    public override void EnterTupleExpression(TupleExpressionContext context)
    {
        _tupleExprDepth++;
        // add expr builder stack on any expression before it's touched
        var exprCount = context.children.Count(x => x is ExpressionContext);
        for (var i = 0; i < exprCount; i++)
        {
            PushExpressionBuilderStack();
        }
        // if inner builder is being used, we push
        if (_tupleExprInnerStore != null)
        {
            AddOpCode(new PushStackOpCode(_tupleExprInnerStore.Value));
            _tupleInnerStorePushDepths.Add(_tupleExprDepth);
        }
    }

    public override void ExitTupleExpression(TupleExpressionContext context)
    {
        _tupleExprDepth--;
        // if we enter back in top level
        if (_tupleExprDepth == 1)
        {
            // entering depth of 1 again means there is something in inner
            Debug.Assert(_tupleExprInnerStore != null, nameof(_tupleExprInnerStore) + " != null");
            if (_tupleExprTopLevelStore == null)
            {
                _tupleExprTopLevelStore = _tupleExprInnerStore;
            }
            else
            {
                var tupleExprInnerStore = _tupleExprInnerStore.Value;
                AddOpCode(new PushTupleOpCode(_tupleExprTopLevelStore.Value, tupleExprInnerStore));
                DeallocateTempRegister(tupleExprInnerStore);
            }
            _tupleExprInnerStore = null;
        }
        else if (_tupleInnerStorePushDepths.Contains(_tupleExprDepth))
        {
            // we need to pop inner builder since this depth contains pushed depth
            Debug.Assert(_tupleExprInnerStore != null, nameof(_tupleExprInnerStore) + " != null");
            var tupleExprInnerStore = _tupleExprInnerStore.Value;
            var tempRegister = AllocateTempRegister();

            AddOpCode(new MoveOpCode(tupleExprInnerStore, tempRegister));
            AddOpCode(new PopStackOpCode(tupleExprInnerStore));
            AddOpCode(new PushTupleOpCode(tupleExprInnerStore, tempRegister));

            DeallocateTempRegister(tempRegister);
            _tupleInnerStorePushDepths.Remove(_tupleExprDepth);
        }
    }

    public override void ExitTupleAssignment(TupleAssignmentContext context)
    {
        Debug.Assert(_tupleExprTopLevelStore != null, nameof(_tupleExprTopLevelStore) + " != null");
        var tupleBuilderStore = _tupleExprTopLevelStore.Value;

        var varName = context.variable().IDENTIFIER_STRING().GetText();
        AddOpCode(new SetVariableOpCode(tupleBuilderStore, varName));

        DeallocateTempRegister(tupleBuilderStore);
        _tupleExprTopLevelStore = null;
    }
}