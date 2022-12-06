using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Antlr4.Runtime;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods;
using UniTASPlugin.Movie.ScriptEngine.Exceptions.ParseExceptions;
using UniTASPlugin.Movie.ScriptEngine.MovieModels.Script;
using UniTASPlugin.Movie.ScriptEngine.OpCodes;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.BitwiseOps;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Jump;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Maths;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Method;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.RegisterSet;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Scope;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.StackOp;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Tuple;
using UniTASPlugin.Movie.ScriptEngine.Parsers.MovieScriptParser.Expressions;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;
using static MovieScriptDefaultGrammarParser;

namespace UniTASPlugin.Movie.ScriptEngine.Parsers.MovieScriptParser;

public class DefaultGrammarListenerCompiler : MovieScriptDefaultGrammarBaseListener
{
    private readonly EngineExternalMethod[] _externalMethods;

    public DefaultGrammarListenerCompiler(IEnumerable<EngineExternalMethod> externMethods)
    {
        _externalMethods = externMethods.ToArray();
    }

    private class MethodBuilder
    {
        public string Name { get; }
        public List<OpCodeBase> OpCodes { get; } = new();
        public int ReturnCount { get; set; } = -1;
        public int ArgCount { get; set; } = -1;

        public MethodBuilder(string name)
        {
            Name = name;
        }

        public ScriptMethodModel GetFinalResult()
        {
            return new ScriptMethodModel(Name, OpCodes);
        }
    }

    private readonly List<OpCodeBase> _mainBuilder = new();
    private int _mainMethodReturnCount = -1;
    private readonly List<MethodBuilder> _builtMethods = new();
    private readonly Stack<MethodBuilder> _methodBuilders = new();

    private OpCodeBuildingType _buildingType = OpCodeBuildingType.BuildingMainMethod;

    private readonly Stack<List<ExpressionBase>> _expressionBuilders = new();

    private readonly bool[] _reservedTempRegister = new bool[RegisterType.Temp8 - RegisterType.Temp0 + 1];
    private readonly Stack<List<int>> _reservedRegisterStackTrack = new();

    private int _tupleExprDepth;
    private RegisterType? _tupleExprTopLevelStore;
    private RegisterType? _tupleExprInnerStore;
    private readonly List<int> _tupleInnerStorePushDepths = new();
    private int _tupleExprCount;

    private readonly Stack<KeyValuePair<KeyValuePair<int, RegisterType>, string>> _ifNotTrueOffsets = new();
    private readonly Stack<KeyValuePair<List<int>, string>> _endOfIfExprOffsets = new();

    private readonly Stack<KeyValuePair<int, string>> _endOfLoopExprOffset =
        new();

    private readonly Stack<KeyValuePair<List<int>, string>> _endOfLoopOffsets = new();
    private readonly Stack<KeyValuePair<int, string>> _startOfLoopOffsets = new();
    private readonly Stack<RegisterType> _loopExprUsingRegisters = new();
    private readonly Stack<KeyValuePair<int, string>> _loopScopeDepth = new();

    private bool _methodCallReturnValueUsed;
    private int _methodArgCount;

    public IEnumerable<ScriptMethodModel> Compile()
    {
        // safety checks
        Debug.Assert(!_reservedTempRegister.Any(x => x),
            "Reserved temporary register is still being used, means something forgot to deallocate it");
        Debug.Assert(_expressionBuilders.Count == 0,
            "Expression builder stack should be empty, something forgot to use it or we allocated too much stack");
        Debug.Assert(_ifNotTrueOffsets.Count == 0, "Offset storage must be empty, something went wrong");
        Debug.Assert(_endOfIfExprOffsets.Count == 0, "Offset storage must be empty, something went wrong");
        Debug.Assert(_endOfLoopOffsets.Count == 0, "Offset storage must be empty, something went wrong");
        Debug.Assert(_startOfLoopOffsets.Count == 0, "Offset storage must be empty, something went wrong");
        Debug.Assert(_loopExprUsingRegisters.Count == 0, "Loop register storage must be empty, something went wrong");

        var methods = new List<ScriptMethodModel> { new(null, _mainBuilder) };
        methods.AddRange(_builtMethods.Select(x => x.GetFinalResult()));
        return methods;
    }

    private string CurrentBuildingMethodName()
    {
        return _buildingType switch
        {
            OpCodeBuildingType.BuildingMethod => _methodBuilders.Peek().Name,
            OpCodeBuildingType.BuildingMainMethod => null,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void PushUsingTempRegisters()
    {
        _reservedRegisterStackTrack.Push(new());
        for (var i = 0; i < _reservedTempRegister.Length; i++)
        {
            var @using = _reservedTempRegister[i];
            var register = RegisterType.Temp0 + i;
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
            var register = RegisterType.Temp0 + usingIndex;
            AddOpCode(new PopStackOpCode(register));
            _reservedTempRegister[usingIndex] = true;
        }
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
        var singularExpressionStorage = new Stack<ExpressionBase>();
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

                    // only store expression if op type contains information (such as cast type)
                    if (expr is CastExpression)
                    {
                        singularExpressionStorage.Push(expr);
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
            if (op.Value != OperationType.FlipNegative &&
                op.Value != OperationType.Not &&
                op.Value != OperationType.Cast &&
                right == null)
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
                case OperationType.Cast:
                    // use singularExpressionStorage
                    AddOpCode(new CastOpCode((singularExpressionStorage.Pop() as CastExpression).ValueType, usingLeft,
                        usingResult));
                    break;
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
        Debug.Assert(expressionBuilder.Count == 1,
            "There should be only a single evaluated expression, or a const, or some method call left");
        Debug.Assert(singularExpressionStorage.Count == 0, "The singular expression storage must be used completely");
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
            return RegisterType.Temp0 + i;
        }

        throw new InvalidOperationException("ran out of temp registers, should never happen");
    }

    private void DeallocateTempRegister(RegisterType register)
    {
        if (register is > RegisterType.Temp8 or < RegisterType.Temp0)
        {
            return;
        }

        _reservedTempRegister[(int)register] = false;
    }

    private void UpdateAndCheckReturnExprCount(bool hasExpr, bool hasTupleExpr)
    {
        // main doesn't matter what gets returned from it
        if (_buildingType is OpCodeBuildingType.BuildingMainMethod) return;

        var currentCount = _buildingType switch
        {
            OpCodeBuildingType.BuildingMainMethod => _mainMethodReturnCount,
            OpCodeBuildingType.BuildingMethod => _methodBuilders.Peek().ReturnCount,
            _ => throw new ArgumentOutOfRangeException()
        };

        // update count if not init
        var returnCount = hasExpr ? 1 : hasTupleExpr ? _tupleExprCount : 0;
        if (currentCount < 0)
        {
            switch (_buildingType)
            {
                case OpCodeBuildingType.BuildingMainMethod:
                    _mainMethodReturnCount = returnCount;
                    break;
                case OpCodeBuildingType.BuildingMethod:
                    _methodBuilders.Peek().ReturnCount = returnCount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return;
        }

        // check count violation
        if (currentCount != returnCount)
        {
            throw new MethodReturnCountNotMatchingException();
        }
    }

    private void AddOpCode(OpCodeBase opCode)
    {
        switch (_buildingType)
        {
            case OpCodeBuildingType.BuildingMainMethod:
                _mainBuilder.Add(opCode);
                break;
            case OpCodeBuildingType.BuildingMethod:
                _methodBuilders.Peek().OpCodes.Add(opCode);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private int GetOpCodeInsertLocation()
    {
        return _buildingType switch
        {
            OpCodeBuildingType.BuildingMainMethod => _mainBuilder.Count,
            OpCodeBuildingType.BuildingMethod => _methodBuilders.Peek().OpCodes.Count,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void InsertOpCodeAndUpdateOffset(int index, OpCodeBase opCode)
    {
        switch (_buildingType)
        {
            case OpCodeBuildingType.BuildingMainMethod:
                _mainBuilder.Insert(index, opCode);
                break;
            case OpCodeBuildingType.BuildingMethod:
                _methodBuilders.Peek().OpCodes.Insert(index, opCode);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // update offsets
        var tempMoveList = new List<KeyValuePair<KeyValuePair<int, RegisterType>, string>>();
        var tempMoveList2 = new List<KeyValuePair<List<int>, string>>();
        var tempMoveList3 = new List<KeyValuePair<int, string>>();

        while (_ifNotTrueOffsets.Count > 0)
        {
            var ifNotTrueOffset = _ifNotTrueOffsets.Pop();
            var offset = ifNotTrueOffset.Key.Key;
            if (ifNotTrueOffset.Value == CurrentBuildingMethodName() && offset > index)
            {
                offset++;
            }

            tempMoveList.Add(new(new(offset, ifNotTrueOffset.Key.Value), ifNotTrueOffset.Value));
        }

        foreach (var tempMove in tempMoveList)
        {
            _ifNotTrueOffsets.Push(tempMove);
        }

        while (_endOfIfExprOffsets.Count > 0)
        {
            var endOfIfExprOffset = _endOfIfExprOffsets.Pop();
            var offsets = endOfIfExprOffset.Key;
            if (endOfIfExprOffset.Value == CurrentBuildingMethodName())
            {
                for (var i = 0; i < offsets.Count; i++)
                {
                    var offset = offsets[i];
                    if (offset > index)
                    {
                        offsets[i]++;
                    }
                }
            }

            tempMoveList2.Add(new(offsets, endOfIfExprOffset.Value));
        }

        foreach (var tempMove in tempMoveList2)
        {
            _endOfIfExprOffsets.Push(tempMove);
        }

        tempMoveList2.Clear();
        while (_endOfLoopOffsets.Count > 0)
        {
            var endOfLoopOffset = _endOfLoopOffsets.Pop();
            var offsets = endOfLoopOffset.Key;
            if (endOfLoopOffset.Value == CurrentBuildingMethodName())
            {
                for (var i = 0; i < offsets.Count; i++)
                {
                    var offset = offsets[i];
                    if (offset > index)
                    {
                        offsets[i]++;
                    }
                }
            }

            tempMoveList2.Add(new(offsets, endOfLoopOffset.Value));
        }

        foreach (var tempMove in tempMoveList2)
        {
            _endOfLoopOffsets.Push(tempMove);
        }

        while (_startOfLoopOffsets.Count > 0)
        {
            var startOfLoopOffset = _startOfLoopOffsets.Pop();
            var offset = startOfLoopOffset.Key;
            offset++;
            tempMoveList3.Add(new(offset, startOfLoopOffset.Value));
        }

        foreach (var tempMove in tempMoveList3)
        {
            _startOfLoopOffsets.Push(tempMove);
        }
    }

    public override void EnterMethodDef(MethodDefContext context)
    {
        var methodName = context.IDENTIFIER_STRING().GetText();
        _buildingType = OpCodeBuildingType.BuildingMethod;
        _methodBuilders.Push(new(methodName));
    }

    public override void ExitMethodDef(MethodDefContext context)
    {
        _builtMethods.Add(_methodBuilders.Pop());
        if (_methodBuilders.Count == 0)
        {
            _buildingType = OpCodeBuildingType.BuildingMainMethod;
        }
    }

    public override void ExitMethodDefArgs(MethodDefArgsContext context)
    {
        var argName = context.IDENTIFIER_STRING().GetText();
        AddOpCode(new PopArgOpCode(RegisterType.Temp0));
        AddOpCode(new SetVariableOpCode(RegisterType.Temp0, argName));
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
            AddExpression(new ConstExpression(new StringValueType(value.Substring(1, value.Length - 2))));
        }
        else if (context.methodCall() != null)
        {
            var methodName = context.methodCall().IDENTIFIER_STRING().GetText();
            CallMethod(methodName);
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
        DeallocateTempRegister(usingRegister);
    }

    public override void EnterFrameAdvance(FrameAdvanceContext context)
    {
        AddOpCode(new FrameAdvanceOpCode());
    }

    public override void EnterMethodCall(MethodCallContext context)
    {
        _methodArgCount = 0;
    }

    private void CallMethod(string methodName)
    {
        PushUsingTempRegisters();

        // validate method existence
        var builtMethodFoundIndex = _builtMethods.FindIndex(x => x.Name == methodName);
        var methodBuildersFoundIndex = -1;
        if (builtMethodFoundIndex < 0)
        {
            methodBuildersFoundIndex = _methodBuilders.ToList().FindIndex(x => x.Name == methodName);
        }

        var externalMethodsFoundIndex = -1;
        if (methodBuildersFoundIndex < 0)
        {
            externalMethodsFoundIndex = _externalMethods.ToList().FindIndex(x => x.Name == methodName);
        }

        if (builtMethodFoundIndex < 0 && methodBuildersFoundIndex < 0 && externalMethodsFoundIndex < 0)
        {
            throw new UsingUndefinedMethodException(methodName);
        }

        var builtMethodFound = builtMethodFoundIndex > -1 ? _builtMethods[builtMethodFoundIndex] : null;
        var methodBuildersFound =
            methodBuildersFoundIndex > -1 ? _methodBuilders.ElementAt(methodBuildersFoundIndex) : null;
        var externalMethodsFound =
            externalMethodsFoundIndex > -1 ? _externalMethods.ElementAt(externalMethodsFoundIndex) : null;

        // validate return existing
        var returnCount = builtMethodFound?.ReturnCount ??
                          (methodBuildersFound?.ReturnCount ??
                           (externalMethodsFound?.ArgReturnCount ?? throw new NotImplementedException()));
        if (_methodCallReturnValueUsed && returnCount == 0)
        {
            throw new MethodHasNoReturnValueException(methodName);
        }

        // validate args equality
        var methodArgCount = builtMethodFound?.ArgCount ??
                             (methodBuildersFound?.ArgCount ?? externalMethodsFound.ArgCount);

        if (methodArgCount != -1 && methodArgCount != _methodArgCount)
        {
            throw new InvokingArgsNotMatchingMethodDefException(methodName, methodArgCount, _methodArgCount);
        }

        AddOpCode(new GotoMethodOpCode(methodName));
        PopUsingTempRegisters();
    }

    public override void ExitMethodCall(MethodCallContext context)
    {
        // store if return value is used
        _methodCallReturnValueUsed = context.Parent is not ActionWithSeparatorContext;
    }

    public override void EnterMethodCallArgs(MethodCallArgsContext context)
    {
        if (context.expression() != null)
        {
            PushExpressionBuilderStack();
        }

        _methodArgCount++;
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
        // if we building method args, push to arg stack
        if (context.Parent is MethodCallArgsContext)
        {
            var argExpr = BuildExpressionOpCodes();
            AddOpCode(new PushArgOpCode(argExpr));
            DeallocateTempRegister(argExpr);
            return;
        }

        // only operate tuple stuff when this expression is evaluated
        if (context.Parent is not TupleExpressionContext) return;

        var resultRegister = BuildExpressionOpCodes();

        // if top level
        if (_tupleExprDepth == 1)
        {
            // we allow top level if top level is null
            if (_tupleExprTopLevelStore == null)
            {
                _tupleExprTopLevelStore = AllocateTempRegister();
                AddOpCode(new ClearTupleOpCode(_tupleExprTopLevelStore.Value));
            }

            AddOpCode(new PushTupleOpCode(_tupleExprTopLevelStore.Value, resultRegister));
            DeallocateTempRegister(resultRegister);

            return;
        }

        // we use expr result register as builder directly if builder is null
        // we push expr result register on builder if builder isn't null
        if (_tupleExprInnerStore == null)
        {
            _tupleExprInnerStore = resultRegister;
            AddOpCode(new ClearTupleOpCode(resultRegister));
        }

        AddOpCode(new PushTupleOpCode(_tupleExprInnerStore.Value, resultRegister));

        if (_tupleExprInnerStore != null)
        {
            DeallocateTempRegister(resultRegister);
        }
    }

    public override void EnterCastExpression(CastExpressionContext context)
    {
        var castTypeExpr = context.basicType();

        BasicValueType castType;
        if (castTypeExpr.toBool != null)
        {
            castType = BasicValueType.Bool;
        }
        else if (castTypeExpr.toFloat != null)
        {
            castType = BasicValueType.Float;
        }
        else if (castTypeExpr.toInt != null)
        {
            castType = BasicValueType.Int;
        }
        else if (castTypeExpr.toString != null)
        {
            castType = BasicValueType.String;
        }
        else
        {
            throw new NotImplementedException("Forgot to implement basic value type casting");
        }

        AddExpression(new CastExpression(castType));
    }

    public override void EnterTupleExpression(TupleExpressionContext context)
    {
        // is this the first tuple expr?
        if (context.Parent is not TupleExpressionContext)
        {
            _tupleExprCount = 0;
        }

        _tupleExprDepth++;
        // add expr builder stack on any expression before it's touched
        var exprCount = context.children.Count(x => x is ExpressionContext);
        for (var i = 0; i < exprCount; i++)
        {
            PushExpressionBuilderStack();
        }

        _tupleExprCount += exprCount;

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

        // handle method call args
        if (context.Parent is MethodCallArgsContext)
        {
            Debug.Assert(_tupleExprTopLevelStore != null, nameof(_tupleExprTopLevelStore) + " != null");
            var tupleBuilderStore = _tupleExprTopLevelStore.Value;

            AddOpCode(new PushArgOpCode(tupleBuilderStore));

            DeallocateTempRegister(tupleBuilderStore);
            _tupleExprTopLevelStore = null;
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

    public override void EnterIfStatement(IfStatementContext context)
    {
        PushExpressionBuilderStack();
        _endOfIfExprOffsets.Push(new(new(), CurrentBuildingMethodName()));
    }

    public override void ExitIfStatement(IfStatementContext context)
    {
        var builtOffsets = _endOfIfExprOffsets.Pop();
        var indexes = builtOffsets.Key;

        foreach (var index in indexes)
        {
            InsertOpCodeAndUpdateOffset(index, new JumpOpCode(GetOpCodeInsertLocation() + 1));
        }
    }

    private void InsertIfNotTrueJump()
    {
        var ifNotTrueOffsetRegisterBuildType = _ifNotTrueOffsets.Pop();
        var ifNotTrueOffsetRegister = ifNotTrueOffsetRegisterBuildType.Key;

        var index = ifNotTrueOffsetRegister.Key;
        var exprRegister = ifNotTrueOffsetRegister.Value;

        InsertOpCodeAndUpdateOffset(index, new JumpIfFalse(GetOpCodeInsertLocation() + 1, exprRegister));
    }

    public override void EnterElseIfStatement(ElseIfStatementContext context)
    {
        PushExpressionBuilderStack();
        InsertIfNotTrueJump();
    }

    public override void EnterElseStatement(ElseStatementContext context)
    {
        InsertIfNotTrueJump();
    }

    public override void EnterScopedProgram(ScopedProgramContext context)
    {
        switch (context.Parent)
        {
            case IfStatementContext or ElseIfStatementContext:
            {
                var register = BuildExpressionOpCodes();
                DeallocateTempRegister(register);
                _ifNotTrueOffsets.Push(
                    new KeyValuePair<KeyValuePair<int, RegisterType>, string>(
                        new KeyValuePair<int, RegisterType>(GetOpCodeInsertLocation(), register),
                        CurrentBuildingMethodName()));
                break;
            }
            case LoopContext:
            {
                _endOfLoopOffsets.Push(new KeyValuePair<List<int>, string>(new(), CurrentBuildingMethodName()));
                _loopScopeDepth.Push(new KeyValuePair<int, string>(0, CurrentBuildingMethodName()));

                var register = BuildExpressionOpCodes();
                DeallocateTempRegister(register);
                // this register used for storing loop count
                _loopExprUsingRegisters.Push(register);

                // for jumping to start of loop
                _startOfLoopOffsets.Push(new(GetOpCodeInsertLocation(), CurrentBuildingMethodName()));

                _endOfLoopExprOffset.Push(
                    new KeyValuePair<int, string>(GetOpCodeInsertLocation(), CurrentBuildingMethodName()));

                // opcodes for loop logic
                // we use hardcoded temp registers since loop count is pushed anyway
                AddOpCode(new ConstToRegisterOpCode(RegisterType.Temp1, new IntValueType(1)));
                AddOpCode(new SubOpCode(RegisterType.Temp0, RegisterType.Temp0, RegisterType.Temp1));
                AddOpCode(new PushStackOpCode(RegisterType.Temp0));

                break;
            }
        }

        if (context.Parent is not MethodDefContext)
        {
            AddOpCode(new EnterScopeOpCode());
        }

        if (context.Parent is not LoopContext && _loopScopeDepth.Count > 0 &&
            _loopScopeDepth.Peek().Value == CurrentBuildingMethodName())
        {
            var loop = _loopScopeDepth.Peek();
            loop = new KeyValuePair<int, string>(loop.Key + 1, loop.Value);
            _loopScopeDepth.Push(loop);
        }
    }

    public override void ExitScopedProgram(ScopedProgramContext context)
    {
        switch (context.Parent)
        {
            case IfStatementContext ifStatement when
                (ifStatement.elseIfStatement() != null || ifStatement.elseStatement() != null):
            case ElseIfStatementContext elseIfStatement when (elseIfStatement.elseIfStatement() != null ||
                                                              elseIfStatement.elseStatement() != null):
            {
                AddOpCode(new ExitScopeOpCode());

                var buildingOffsets = _endOfIfExprOffsets.Peek();
                buildingOffsets.Key.Add(GetOpCodeInsertLocation());
                break;
            }
            case LoopContext:
            {
                _loopScopeDepth.Pop();
                var loopCountStoreRegister = _loopExprUsingRegisters.Pop();

                AddOpCode(new ExitScopeOpCode());

                // loop ending stuff
                var startIndex = _startOfLoopOffsets.Pop().Key;

                AddOpCode(new PopStackOpCode(loopCountStoreRegister));
                AddOpCode(new JumpOpCode(startIndex - GetOpCodeInsertLocation() - 1));

                // jumps from start of loop, and middle of loop (break)

                var endOfLoopExprOffset = _endOfLoopExprOffset.Pop();
                var loopExprJumpIndex = endOfLoopExprOffset.Key;

                InsertOpCodeAndUpdateOffset(loopExprJumpIndex,
                    new JumpIfEqZero(GetOpCodeInsertLocation() + 1, loopCountStoreRegister));

                var endOfLoopOffsets = _endOfLoopOffsets.Pop();
                var indexes = endOfLoopOffsets.Key;

                foreach (var index in indexes)
                {
                    InsertOpCodeAndUpdateOffset(index, new JumpOpCode(GetOpCodeInsertLocation() + 1));
                }

                break;
            }
            // in case of ending if else statement
            case IfStatementContext or ElseIfStatementContext when
                !context.children.Any(x => x is ElseIfStatementContext or ElseStatementContext):
                InsertIfNotTrueJump();
                AddOpCode(new ExitScopeOpCode());
                break;
            case not MethodDefContext:
                AddOpCode(new ExitScopeOpCode());
                break;
        }

        if (context.Parent is not LoopContext && _loopScopeDepth.Count > 0 &&
            _loopScopeDepth.Peek().Value == CurrentBuildingMethodName())
        {
            var loop = _loopScopeDepth.Pop();
            _loopScopeDepth.Push(new KeyValuePair<int, string>(loop.Key - 1, loop.Value));
        }
    }

    public override void EnterLoop(LoopContext context)
    {
        // add the loop expression
        PushExpressionBuilderStack();
    }

    public override void EnterBreakAction(BreakActionContext context)
    {
        // validate if in loop
        if (_loopScopeDepth.Count == 0 || _loopScopeDepth.Peek().Value != CurrentBuildingMethodName())
        {
            throw new UsingLoopActionOutsideOfLoopException("continue");
        }

        // depending on the "depth" of the continue, we must exit scope the right amount of times
        var depth = _loopScopeDepth.Peek().Key + 1;
        for (var i = 0; i < depth; i++)
        {
            AddOpCode(new ExitScopeOpCode());
        }

        AddOpCode(new PopStackOpCode(_loopExprUsingRegisters.Peek()));
        _endOfLoopOffsets.Peek().Key.Add(GetOpCodeInsertLocation());
    }

    public override void EnterContinueAction(ContinueActionContext context)
    {
        // validate if in loop
        if (_loopScopeDepth.Count == 0 || _loopScopeDepth.Peek().Value != CurrentBuildingMethodName())
        {
            throw new UsingLoopActionOutsideOfLoopException("continue");
        }

        // depending on the "depth" of the continue, we must exit scope the right amount of times
        var depth = _loopScopeDepth.Peek().Key + 1;
        for (var i = 0; i < depth; i++)
        {
            AddOpCode(new ExitScopeOpCode());
        }

        AddOpCode(new PopStackOpCode(_loopExprUsingRegisters.Peek()));
        AddOpCode(new JumpOpCode(_startOfLoopOffsets.Peek().Key));
    }

    public override void EnterReturnAction(ReturnActionContext context)
    {
        if (context.expression() != null)
        {
            PushExpressionBuilderStack();
        }
    }

    public override void ExitReturnAction(ReturnActionContext context)
    {
        UpdateAndCheckReturnExprCount(context.expression() != null, context.tupleExpression() != null);

        if (context.expression() != null)
        {
            var register = BuildExpressionOpCodes();

            if (register is not RegisterType.Ret)
            {
                AddOpCode(new MoveOpCode(register, RegisterType.Ret));
            }

            AddOpCode(new ReturnOpCode());

            DeallocateTempRegister(register);
        }
        else if (context.tupleExpression() != null)
        {
            Debug.Assert(_tupleExprTopLevelStore != null, nameof(_tupleExprTopLevelStore) + " != null");
            var tupleBuilderStore = _tupleExprTopLevelStore.Value;

            if (tupleBuilderStore is not RegisterType.Ret)
            {
                AddOpCode(new MoveOpCode(tupleBuilderStore, RegisterType.Ret));
            }

            AddOpCode(new ReturnOpCode());

            DeallocateTempRegister(tupleBuilderStore);
            _tupleExprTopLevelStore = null;
        }
        else
        {
            AddOpCode(new ReturnOpCode());
        }
    }

    public override void ExitVariableTupleSeparation(VariableTupleSeparationContext context)
    {
        // initialize
        RegisterType tupleRegister;

        if (context.tupleExpression() != null)
        {
            Debug.Assert(_tupleExprTopLevelStore != null, nameof(_tupleExprTopLevelStore) + " != null");
            tupleRegister = _tupleExprTopLevelStore.Value;
        }
        else if (context.methodCall() != null)
        {
            CallMethod(context.methodCall().IDENTIFIER_STRING().GetText());
            tupleRegister = RegisterType.Ret;
        }
        else if (context.variable() != null)
        {
            tupleRegister = AllocateTempRegister();
            AddOpCode(new VarToRegisterOpCode(tupleRegister, context.variable().IDENTIFIER_STRING().GetText()));
        }
        else
        {
            throw new NotImplementedException();
        }

        // separate
        var tupleTempStore = AllocateTempRegister();

        // handle first one manually, im lazy
        AddOpCode(new PopTupleOpCode(tupleTempStore, tupleRegister));
        if (context.firstVar.varName != null)
        {
            AddOpCode(new SetVariableOpCode(tupleTempStore, context.firstVar.varName.Text));
        }

        // handle all defines
        foreach (var var in context._vars)
        {
            AddOpCode(new PopTupleOpCode(tupleTempStore, tupleRegister));
            if (var.varName == null) continue;
            AddOpCode(new SetVariableOpCode(tupleTempStore, var.varName.Text));
        }

        // clean up
        if (context.tupleExpression() != null)
        {
            DeallocateTempRegister(tupleRegister);
            _tupleExprTopLevelStore = null;
        }
        else if (context.variable() != null)
        {
            DeallocateTempRegister(tupleRegister);
        }

        DeallocateTempRegister(tupleTempStore);
    }
}