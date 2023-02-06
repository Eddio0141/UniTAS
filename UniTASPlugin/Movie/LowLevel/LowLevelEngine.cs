using System;
using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.Movie.EngineMethods;
using UniTASPlugin.Movie.Exceptions.ScriptEngineExceptions;
using UniTASPlugin.Movie.LowLevel.OpCodes;
using UniTASPlugin.Movie.LowLevel.OpCodes.BitwiseOps;
using UniTASPlugin.Movie.LowLevel.OpCodes.Jump;
using UniTASPlugin.Movie.LowLevel.OpCodes.Logic;
using UniTASPlugin.Movie.LowLevel.OpCodes.Maths;
using UniTASPlugin.Movie.LowLevel.OpCodes.Method;
using UniTASPlugin.Movie.LowLevel.OpCodes.RegisterSet;
using UniTASPlugin.Movie.LowLevel.OpCodes.Scope;
using UniTASPlugin.Movie.LowLevel.OpCodes.StackOp;
using UniTASPlugin.Movie.LowLevel.OpCodes.Tuple;
using UniTASPlugin.Movie.LowLevel.Register;
using UniTASPlugin.Movie.MovieModels.Script;
using UniTASPlugin.Movie.ValueTypes;
using ValueType = UniTASPlugin.Movie.ValueTypes.ValueType;

namespace UniTASPlugin.Movie.LowLevel;

public partial class LowLevelEngine
{
    private readonly Register.Register[] _registers;
    private readonly Stack<Register.Register>[] _registerStack;
    private readonly Stack<List<ValueType>> _argStack = new();

    private readonly OpCode[] _mainMethod;
    private readonly List<ScriptMethodModel> _methods;
    private readonly List<EngineExternalMethod> _externMethods;

    // current method info
    private int _pc;
    private int _methodIndex = -1;
    private Stack<List<VariableInfo>> _vars = new();

    // basically global vars
    private readonly Stack<List<VariableInfo>> _mainVars = new();

    // external vars
    private readonly Stack<List<VariableInfo>> _externalVars;

    // storage for "paused" methods
    private readonly Stack<MethodInfo> _methodStack = new();

    public bool FinishedExecuting { get; private set; }

    public LowLevelEngine(ScriptModel script, IEnumerable<EngineExternalMethod> methods,
        LowLevelEngine upperEngine = null)
    {
        _externalVars = upperEngine == null ? new() : upperEngine._mainVars;

        var registerCount = Enum.GetNames(typeof(RegisterType)).Length;
        _registers = new Register.Register[registerCount];
        _registerStack = new Stack<Register.Register>[registerCount];
        for (var i = 0; i < registerCount; i++)
        {
            _registers[i] = new();
            _registerStack[i] = new();
        }

        _mainMethod = script.MainMethod.OpCodes;
        _methods = script.Methods.ToList();
        _externMethods = methods.ToList();
        _mainVars.Push(new());
    }

    public void Reset()
    {
        var registerCount = Enum.GetNames(typeof(RegisterType)).Length;
        for (var i = 0; i < registerCount; i++)
        {
            _registers[i] = new();
            _registerStack[i] = new();
        }

        _argStack.Clear();
        _pc = 0;
        _methodIndex = -1;
        _vars.Clear();
        _mainVars.Clear();
        _mainVars.Push(new());
        _methodStack.Clear();
        FinishedExecuting = false;
    }

    private List<ValueType> GetVariable(string name)
    {
        // vars defined in external can be accessed from anywhere
        // vars defined in main can be found from anywhere
        // vars defined in method is stored in anywhere in that method
        // vars in scopes are pushed on a stack, popped later on scope exit

        foreach (var varStack in _externalVars)
        {
            foreach (var var in varStack)
            {
                if (var.Name == name)
                {
                    return var.Value;
                }
            }
        }

        foreach (var varStack in _mainVars)
        {
            foreach (var var in varStack)
            {
                if (var.Name == name)
                {
                    return var.Value;
                }
            }
        }

        foreach (var varStack in _vars)
        {
            foreach (var var in varStack)
            {
                if (var.Name == name)
                {
                    return var.Value;
                }
            }
        }

        throw new UsingUndefinedVariableException(name);
    }

    /// <summary>
    /// Validates _pc and _method_index
    /// </summary>
    /// <returns>If _pc was updated in here</returns>
    private bool ValidatePcOffset()
    {
        var opCodes = _methodIndex < 0 ? _mainMethod : _methods[_methodIndex].OpCodes;
        if (_pc < opCodes.Length) return false;

        // if this is main, movie end
        if (_methodIndex < 0)
        {
            FinishedExecuting = true;
            return true;
        }

        // if this is method, exit to outer method
        while (true)
        {
            var method = _methodStack.Pop();
            _pc = method.Pc;
            _methodIndex = method.MethodIndex;

            // re-validate pc
            opCodes = _methodIndex < 0 ? _mainMethod : _methods[_methodIndex].OpCodes;
            if (_pc < opCodes.Length)
            {
                if (_methodIndex >= 0)
                {
                    _vars = method.Vars;
                }

                break;
            }

            // are we finished executing everything?
            if (_methodIndex < 0)
            {
                FinishedExecuting = true;
                return true;
            }
        }

        return true;
    }

    private struct LeftRightResultValues<TValue>
        where TValue : ValueType
    {
        public TValue Left { get; }
        public TValue Right { get; }
        public Register.Register Result { get; }

        public LeftRightResultValues(TValue left, TValue right, Register.Register result)
        {
            Left = left;
            Right = right;
            Result = result;
        }
    }

    private struct LeftRightResultValuesRaw
    {
        public ValueType Left { get; }
        public ValueType Right { get; }
        public Register.Register Result { get; }

        public LeftRightResultValuesRaw(ValueType left, ValueType right, Register.Register result)
        {
            Left = left;
            Right = right;
            Result = result;
        }
    }

    private void ValidateRegisterNonTuple<T>(Register.Register register)
    {
        if (register.IsTuple)
        {
            throw new ValueTypeMismatchException(typeof(T).ToString(), register.InnerValue.ToString());
        }
    }

    private void ValidateRegisterNonTuple(Register.Register register)
    {
        if (register.IsTuple)
        {
            throw new ValueTypeMismatchException("Non-tuple type", "Tuple type");
        }
    }

    private LeftRightResultValues<T> ValidateTypeAndGetRegister<T>(RegisterType left, RegisterType? result = null)
        where T : ValueType
    {
        var leftRegister = _registers[(int)left];
        var leftValue = leftRegister.InnerValue;
        ValidateRegisterNonTuple<T>(leftRegister);

        if (leftValue is not T newLeft)
        {
            var valueType = leftValue.GetType().ToString();

            throw new ValueTypeMismatchException(typeof(T).ToString(), valueType);
        }

        return new(newLeft, null, result == null ? null : _registers[(int)result]);
    }

    private LeftRightResultValues<T> ValidateTypeAndGetRegister<T>(RegisterType left, RegisterType right,
        RegisterType result)
        where T : ValueType
    {
        var leftRegister = _registers[(int)left];
        var leftValue = leftRegister.InnerValue;
        ValidateRegisterNonTuple<T>(leftRegister);
        var rightRegister = _registers[(int)right];
        var rightValue = rightRegister.InnerValue;
        ValidateRegisterNonTuple<T>(rightRegister);

        if (leftValue is not T newLeft || rightValue is not T newRight)
        {
            var valueType = leftValue is not T ? leftValue.GetType().ToString() : rightValue.GetType().ToString();

            throw new ValueTypeMismatchException(typeof(T).ToString(), valueType);
        }

        return new(newLeft, newRight, _registers[(int)result]);
    }

    private LeftRightResultValuesRaw ValidateTypeAndGetRegister(RegisterType left, RegisterType right,
        RegisterType? result = null)
    {
        var leftRegister = _registers[(int)left];
        var leftValue = leftRegister.InnerValue;
        ValidateRegisterNonTuple(leftRegister);
        var rightRegister = _registers[(int)right];
        var rightValue = rightRegister.InnerValue;
        ValidateRegisterNonTuple(rightRegister);

        if (leftValue is BoolValueType && rightValue is not BoolValueType ||
            leftValue is IntValueType && rightValue is not IntValueType ||
            leftValue is FloatValueType && rightValue is not FloatValueType ||
            leftValue is StringValueType && rightValue is not StringValueType)
        {
            throw new ValueTypeMismatchException(leftValue.GetType().ToString(), rightValue.GetType().ToString());
        }

        return new(leftValue, rightValue, result == null ? null : _registers[(int)result]);
    }

    public void ExecUntilStop(MovieRunner runner)
    {
        var opCodes = _methodIndex < 0 ? _mainMethod : _methods[_methodIndex].OpCodes;

        while (true)
        {
            var opCode = opCodes[_pc];

            switch (opCode)
            {
                case BitwiseAndOpCode bitwiseAndOpCode:
                {
                    var values =
                        ValidateTypeAndGetRegister<IntValueType>(bitwiseAndOpCode.Left, bitwiseAndOpCode.Right,
                            bitwiseAndOpCode.ResultRegister);
                    values.Result.InnerValue = new IntValueType(values.Left.Value & values.Right.Value);
                    _pc++;
                    break;
                }
                case BitwiseOrOpCode bitwiseOrOpCode:
                {
                    var values =
                        ValidateTypeAndGetRegister<IntValueType>(bitwiseOrOpCode.Left, bitwiseOrOpCode.Right,
                            bitwiseOrOpCode.ResultRegister);
                    values.Result.InnerValue = new IntValueType(values.Left.Value | values.Right.Value);
                    _pc++;
                    break;
                }
                case BitwiseShiftLeftOpCode bitwiseShiftLeftOpCode:
                {
                    var values =
                        ValidateTypeAndGetRegister<IntValueType>(bitwiseShiftLeftOpCode.Left,
                            bitwiseShiftLeftOpCode.Right, bitwiseShiftLeftOpCode.ResultRegister);
                    values.Result.InnerValue = new IntValueType(values.Left.Value << values.Right.Value);
                    _pc++;
                    break;
                }
                case BitwiseShiftRightOpCode bitwiseShiftRightOpCode:
                {
                    var values =
                        ValidateTypeAndGetRegister<IntValueType>(bitwiseShiftRightOpCode.Left,
                            bitwiseShiftRightOpCode.Right, bitwiseShiftRightOpCode.ResultRegister);
                    values.Result.InnerValue = new IntValueType(values.Left.Value >> values.Right.Value);
                    _pc++;
                    break;
                }
                case BitwiseXorOpCode bitwiseXorOpCode:
                {
                    var values =
                        ValidateTypeAndGetRegister<IntValueType>(bitwiseXorOpCode.Left, bitwiseXorOpCode.Right,
                            bitwiseXorOpCode.ResultRegister);
                    values.Result.InnerValue = new IntValueType(values.Left.Value ^ values.Right.Value);
                    _pc++;
                    break;
                }
                case CastOpCode castOpCode:
                {
                    var sourceString = _registers[(int)castOpCode.Source].ToString();
                    _registers[(int)castOpCode.Dest].InnerValue = castOpCode.ValueType switch
                    {
                        BasicValueType.Bool => new BoolValueType(bool.Parse(sourceString)),
                        BasicValueType.Float => new FloatValueType(float.Parse(sourceString)),
                        BasicValueType.Int => new IntValueType(int.Parse(sourceString)),
                        BasicValueType.String => new StringValueType(sourceString),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    _pc++;
                    break;
                }
                case FrameAdvanceOpCode:
                {
                    _pc++;
                    // we exit from loop
                    ValidatePcOffset();
                    return;
                }
                case JumpIfEqZero jumpIfEqZero:
                {
                    var values = ValidateTypeAndGetRegister<IntValueType>(jumpIfEqZero.Register);
                    if (values.Left.Value == 0)
                    {
                        _pc += jumpIfEqZero.Offset;
                    }
                    else
                    {
                        _pc++;
                    }

                    break;
                }
                case JumpIfFalse jumpIfFalse:
                {
                    var values = ValidateTypeAndGetRegister<BoolValueType>(jumpIfFalse.Register);
                    if (!values.Left.Value)
                    {
                        _pc += jumpIfFalse.Offset;
                    }
                    else
                    {
                        _pc++;
                    }

                    break;
                }
                case JumpOpCode jumpOpCode:
                {
                    _pc += jumpOpCode.Offset;
                    break;
                }
                case AndOpCode andOpCode:
                {
                    var values =
                        ValidateTypeAndGetRegister<BoolValueType>(andOpCode.Left, andOpCode.Right, andOpCode.Dest);
                    values.Result.InnerValue = new BoolValueType(values.Left.Value && values.Right.Value);
                    break;
                }
                case EqualOpCode equalOpCode:
                {
                    var values = ValidateTypeAndGetRegister(equalOpCode.Left, equalOpCode.Right, equalOpCode.Dest);
                    var res = values.Left switch
                    {
                        BoolValueType boolValueType => boolValueType.Value == ((BoolValueType)values.Right).Value,
                        FloatValueType floatValueType => Math.Abs(floatValueType.Value -
                                                                  ((FloatValueType)values.Right).Value) < 0.0001f,
                        IntValueType intValueType => intValueType.Value == ((IntValueType)values.Right).Value,
                        StringValueType stringValueType => stringValueType.Value ==
                                                           ((StringValueType)values.Right).Value,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    values.Result.InnerValue = new BoolValueType(res);
                    _pc++;
                    break;
                }
                case GreaterEqualOpCode greaterEqualOpCode:
                {
                    try
                    {
                        var values =
                            ValidateTypeAndGetRegister<FloatValueType>(greaterEqualOpCode.Left,
                                greaterEqualOpCode.Right,
                                greaterEqualOpCode.Dest);
                        values.Result.InnerValue = new BoolValueType(values.Left.Value >= values.Right.Value);
                    }
                    catch (ValueTypeMismatchException)
                    {
                        var values = ValidateTypeAndGetRegister<IntValueType>(greaterEqualOpCode.Left,
                            greaterEqualOpCode.Right,
                            greaterEqualOpCode.Dest);
                        values.Result.InnerValue = new BoolValueType(values.Left.Value >= values.Right.Value);
                    }

                    _pc++;
                    break;
                }
                case GreaterOpCode greaterOpCode:
                {
                    try
                    {
                        var values =
                            ValidateTypeAndGetRegister<FloatValueType>(greaterOpCode.Left, greaterOpCode.Right,
                                greaterOpCode.Dest);
                        values.Result.InnerValue = new BoolValueType(values.Left.Value > values.Right.Value);
                    }
                    catch (ValueTypeMismatchException)
                    {
                        var values = ValidateTypeAndGetRegister<IntValueType>(greaterOpCode.Left, greaterOpCode.Right,
                            greaterOpCode.Dest);
                        values.Result.InnerValue = new BoolValueType(values.Left.Value > values.Right.Value);
                    }

                    _pc++;
                    break;
                }
                case LessEqualOpCode lessEqualOpCode:
                {
                    try
                    {
                        var values =
                            ValidateTypeAndGetRegister<FloatValueType>(lessEqualOpCode.Left, lessEqualOpCode.Right,
                                lessEqualOpCode.Dest);
                        values.Result.InnerValue = new BoolValueType(values.Left.Value <= values.Right.Value);
                    }
                    catch (ValueTypeMismatchException)
                    {
                        var values = ValidateTypeAndGetRegister<IntValueType>(lessEqualOpCode.Left,
                            lessEqualOpCode.Right,
                            lessEqualOpCode.Dest);
                        values.Result.InnerValue = new BoolValueType(values.Left.Value <= values.Right.Value);
                    }

                    _pc++;
                    break;
                }
                case LessOpCode lessOpCode:
                {
                    try
                    {
                        var values =
                            ValidateTypeAndGetRegister<FloatValueType>(lessOpCode.Left, lessOpCode.Right,
                                lessOpCode.Dest);
                        values.Result.InnerValue = new BoolValueType(values.Left.Value < values.Right.Value);
                    }
                    catch (ValueTypeMismatchException)
                    {
                        var values = ValidateTypeAndGetRegister<IntValueType>(lessOpCode.Left, lessOpCode.Right,
                            lessOpCode.Dest);
                        values.Result.InnerValue = new BoolValueType(values.Left.Value < values.Right.Value);
                    }

                    _pc++;
                    break;
                }
                case NotEqualOpCode notEqualOpCode:
                {
                    var values = ValidateTypeAndGetRegister(notEqualOpCode.Left, notEqualOpCode.Right,
                        notEqualOpCode.Dest);
                    var res = values.Left switch
                    {
                        BoolValueType boolValueType => boolValueType.Value != ((BoolValueType)values.Right).Value,
                        FloatValueType floatValueType => Math.Abs(floatValueType.Value -
                                                                  ((FloatValueType)values.Right).Value) > 0.0001f,
                        IntValueType intValueType => intValueType.Value != ((IntValueType)values.Right).Value,
                        StringValueType stringValueType => stringValueType.Value !=
                                                           ((StringValueType)values.Right).Value,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    values.Result.InnerValue = new BoolValueType(res);
                    _pc++;
                    break;
                }
                case NotOpCode notOpCode:
                {
                    var values = ValidateTypeAndGetRegister<BoolValueType>(notOpCode.Source, notOpCode.Dest);
                    values.Result.InnerValue = new BoolValueType(!values.Left.Value);
                    _pc++;
                    break;
                }
                case OrOpCode orOpCode:
                {
                    var values =
                        ValidateTypeAndGetRegister<BoolValueType>(orOpCode.Left, orOpCode.Right, orOpCode.Dest);
                    values.Result.InnerValue = new BoolValueType(values.Left.Value || values.Right.Value);
                    _pc++;
                    break;
                }
                case AddOpCode addOpCode:
                {
                    try
                    {
                        var values =
                            ValidateTypeAndGetRegister<FloatValueType>(addOpCode.Left, addOpCode.Right,
                                addOpCode.Result);
                        values.Result.InnerValue = new FloatValueType(values.Left.Value + values.Right.Value);
                    }
                    catch (ValueTypeMismatchException)
                    {
                        var values = ValidateTypeAndGetRegister<IntValueType>(addOpCode.Left, addOpCode.Right,
                            addOpCode.Result);
                        values.Result.InnerValue = new IntValueType(values.Left.Value + values.Right.Value);
                    }

                    _pc++;
                    break;
                }
                case DivOpCode divOpCode:
                {
                    try
                    {
                        var values =
                            ValidateTypeAndGetRegister<FloatValueType>(divOpCode.Left, divOpCode.Right,
                                divOpCode.Result);
                        values.Result.InnerValue = new FloatValueType(values.Left.Value / values.Right.Value);
                    }
                    catch (ValueTypeMismatchException)
                    {
                        var values = ValidateTypeAndGetRegister<IntValueType>(divOpCode.Left, divOpCode.Right,
                            divOpCode.Result);
                        values.Result.InnerValue = new IntValueType(values.Left.Value / values.Right.Value);
                    }

                    _pc++;
                    break;
                }
                case FlipNegativeOpCode flipNegativeOpCode:
                {
                    try
                    {
                        var values = ValidateTypeAndGetRegister<FloatValueType>(flipNegativeOpCode.Source,
                            flipNegativeOpCode.Dest);
                        values.Result.InnerValue = new FloatValueType(-values.Left.Value);
                    }
                    catch (ValueTypeMismatchException)
                    {
                        var values = ValidateTypeAndGetRegister<IntValueType>(flipNegativeOpCode.Source,
                            flipNegativeOpCode.Dest);
                        values.Result.InnerValue = new IntValueType(-values.Left.Value);
                    }

                    _pc++;
                    break;
                }
                case ModOpCode modOpCode:
                {
                    var values = ValidateTypeAndGetRegister<IntValueType>(modOpCode.Left, modOpCode.Right,
                        modOpCode.Result);
                    values.Result.InnerValue = new IntValueType(values.Left.Value % values.Right.Value);
                    _pc++;
                    break;
                }
                case MultOpCode multOpCode:
                {
                    try
                    {
                        var values =
                            ValidateTypeAndGetRegister<FloatValueType>(multOpCode.Left, multOpCode.Right,
                                multOpCode.Result);
                        values.Result.InnerValue = new FloatValueType(values.Left.Value * values.Right.Value);
                    }
                    catch (ValueTypeMismatchException)
                    {
                        var values = ValidateTypeAndGetRegister<IntValueType>(multOpCode.Left, multOpCode.Right,
                            multOpCode.Result);
                        values.Result.InnerValue = new IntValueType(values.Left.Value * values.Right.Value);
                    }

                    _pc++;
                    break;
                }
                case SubOpCode subOpCode:
                {
                    try
                    {
                        var values =
                            ValidateTypeAndGetRegister<FloatValueType>(subOpCode.Left, subOpCode.Right,
                                subOpCode.Result);
                        values.Result.InnerValue = new FloatValueType(values.Left.Value - values.Right.Value);
                    }
                    catch (ValueTypeMismatchException)
                    {
                        var values = ValidateTypeAndGetRegister<IntValueType>(subOpCode.Left, subOpCode.Right,
                            subOpCode.Result);
                        values.Result.InnerValue = new IntValueType(values.Left.Value - values.Right.Value);
                    }

                    _pc++;
                    break;
                }
                case GotoMethodOpCode gotoMethodOpCode:
                {
                    // method exists, which is checked at compile time
                    _pc++;
                    var method = gotoMethodOpCode.MethodName;
                    var foundDefinedMethod = _methods.FindIndex(m => m.Name == method);
                    if (foundDefinedMethod > -1)
                    {
                        _methodStack.Push(new(_pc, _methodIndex, _vars));
                        _pc = 0;
                        _methodIndex = foundDefinedMethod;
                        _vars.Clear();
                        _vars.Push(new());
                        opCodes = _methods[foundDefinedMethod].OpCodes;
                        break;
                    }

                    // call to extern method, requires just a call
                    var externMethod = _externMethods.Find(x => x.Name == method);
                    var resultValue = externMethod.Invoke(
                        _argStack.Reverse().ToList().Select(x => (IEnumerable<ValueType>)x),
                        runner);
                    var resultRegister = _registers[(int)RegisterType.Ret];
                    switch (resultValue.Count)
                    {
                        case 1:
                            resultRegister.InnerValue = resultValue[0];
                            break;
                        case > 1:
                            resultRegister.TupleValues = resultValue;
                            resultRegister.IsTuple = true;
                            break;
                    }

                    _argStack.Clear();
                    break;
                }
                case PopArgOpCode popArgOpCode:
                {
                    _pc++;
                    var arg = _argStack.Pop();
                    var register = _registers[(int)popArgOpCode.Register];
                    if (arg.Count == 1)
                    {
                        register.InnerValue = arg[0];
                    }
                    else
                    {
                        register.TupleValues = arg;
                        register.IsTuple = true;
                    }

                    break;
                }
                case PushArgOpCode pushArgOpCode:
                {
                    _pc++;
                    var value = _registers[(int)pushArgOpCode.RegisterType];
                    _argStack.Push(value.IsTuple
                        ? new(value.TupleValues)
                        : new List<ValueType> { value.InnerValue });
                    break;
                }
                case ReturnOpCode:
                {
                    if (_methodIndex < 0)
                    {
                        FinishedExecuting = true;
                        return;
                    }

                    // return from method
                    var method = _methodStack.Pop();
                    _pc = method.Pc;
                    _methodIndex = method.MethodIndex;
                    opCodes = _methodIndex < 0 ? _mainMethod : _methods[_methodIndex].OpCodes;

                    break;
                }
                case ConstToRegisterOpCode constToRegisterOpCode:
                {
                    _pc++;
                    _registers[(int)constToRegisterOpCode.Register].InnerValue = constToRegisterOpCode.Value;
                    break;
                }
                case MoveOpCode moveOpCode:
                {
                    _pc++;
                    var source = _registers[(int)moveOpCode.Register];
                    var dest = _registers[(int)moveOpCode.Dest];
                    if (source.IsTuple)
                    {
                        dest.IsTuple = true;
                        dest.TupleValues = new();
                        foreach (var value in source.TupleValues)
                        {
                            dest.TupleValues.Add((ValueType)value.Clone());
                        }
                    }
                    else
                    {
                        dest.InnerValue = (ValueType)source.InnerValue.Clone();
                    }

                    break;
                }
                case VarToRegisterOpCode varToRegisterOpCode:
                {
                    _pc++;
                    var register = _registers[(int)varToRegisterOpCode.Register];
                    // get vars
                    var var = GetVariable(varToRegisterOpCode.Name);
                    if (var.Count == 1)
                    {
                        register.InnerValue = var[0];
                    }
                    else
                    {
                        register.TupleValues = var;
                        register.IsTuple = true;
                    }

                    break;
                }
                case EnterScopeOpCode:
                {
                    _pc++;
                    if (_methodIndex < 0)
                    {
                        _mainVars.Push(new());
                    }
                    else
                    {
                        _vars.Push(new());
                    }

                    break;
                }
                case ExitScopeOpCode:
                {
                    _pc++;
                    if (_methodIndex < 0)
                    {
                        _mainVars.Pop();
                    }
                    else
                    {
                        _vars.Pop();
                    }

                    break;
                }
                case SetVariableOpCode setVariableOpCode:
                {
                    _pc++;
                    VariableInfo foundVar = null;

                    foreach (var varStack in _externalVars)
                    {
                        foreach (var var in varStack)
                        {
                            if (var.Name == setVariableOpCode.Name)
                            {
                                foundVar = var;
                                break;
                            }
                        }

                        if (foundVar != null)
                        {
                            break;
                        }
                    }

                    if (foundVar == null)
                    {
                        foreach (var varStack in _mainVars)
                        {
                            foreach (var var in varStack)
                            {
                                if (var.Name == setVariableOpCode.Name)
                                {
                                    foundVar = var;
                                    break;
                                }
                            }

                            if (foundVar != null)
                            {
                                break;
                            }
                        }

                        if (foundVar == null)
                        {
                            foreach (var varStack in _vars)
                            {
                                foreach (var var in varStack)
                                {
                                    if (var.Name == setVariableOpCode.Name)
                                    {
                                        foundVar = var;
                                        break;
                                    }
                                }

                                if (foundVar != null)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    var register = _registers[(int)setVariableOpCode.Register];
                    var registerValue = register.IsTuple
                        ? register.TupleValues
                        : new() { register.InnerValue };
                    if (foundVar == null)
                    {
                        // new var
                        if (_methodIndex < 0)
                        {
                            _mainVars.Peek().Add(new(setVariableOpCode.Name, registerValue));
                        }
                        else
                        {
                            _vars.Peek().Add(new(setVariableOpCode.Name, registerValue));
                        }

                        break;
                    }

                    // update var
                    foundVar.Value = registerValue;

                    break;
                }

                case PopStackOpCode popStackOpCode:
                {
                    _pc++;
                    var poppedRegister = _registerStack[(int)popStackOpCode.Register].Pop();
                    _registers[(int)popStackOpCode.Register] = poppedRegister;
                    break;
                }
                case PushStackOpCode pushStackOpCode:
                {
                    _pc++;
                    var register = _registers[(int)pushStackOpCode.Register];
                    _registerStack[(int)pushStackOpCode.Register].Push((Register.Register)register.Clone());
                    break;
                }
                case PopTupleOpCode popTupleOpCode:
                {
                    _pc++;
                    var source = _registers[(int)popTupleOpCode.Source];
                    var poppedValue = source.TupleValues[0];
                    source.TupleValues.RemoveAt(0);
                    _registers[(int)popTupleOpCode.Dest].InnerValue = poppedValue;
                    break;
                }
                case PushTupleOpCode pushTupleOpCode:
                {
                    _pc++;
                    var source = _registers[(int)pushTupleOpCode.Source];
                    var dest = _registers[(int)pushTupleOpCode.Dest];
                    dest.TupleValues.Add(source.InnerValue);
                    dest.IsTuple = true;

                    break;
                }
                case ClearTupleOpCode clearTupleOpCode:
                {
                    _pc++;
                    _registers[(int)clearTupleOpCode.Register].TupleValues.Clear();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(opCode));
            }

            if (ValidatePcOffset())
            {
                if (FinishedExecuting)
                {
                    return;
                }

                opCodes = _methodIndex < 0 ? _mainMethod : _methods[_methodIndex].OpCodes;
            }
        }
    }
}