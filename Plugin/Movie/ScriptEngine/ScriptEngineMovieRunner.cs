using System;
using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment.Interfaces;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods;
using UniTASPlugin.Movie.ScriptEngine.MovieModels.Script;
using UniTASPlugin.Movie.ScriptEngine.OpCodes;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.BitwiseOps;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Jump;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Logic;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Loop;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Maths;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Method;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.RegisterSet;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Scope;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.StackOp;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Tuple;
using UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine;

public class ScriptEngineMovieRunner : IMovieRunner
{
    private Register[] _registers;
    private OpCodeBase[] _mainMethod;
    private ScriptMethodModel[] _methods;
    private ulong _pc;
    private int _currentMethod;

    public bool MovieEnd { get; private set; }

    private readonly IMovieParser _parser;
    public bool IsRunning { get; private set; }

    private readonly EngineExternalMethodBase[] _externMethods;

    public ScriptEngineMovieRunner(IMovieParser parser, IGetDefinedMethods getDefinedMethods)
    {
        IsRunning = false;
        _parser = parser;
        _externMethods = getDefinedMethods.GetExternMethods().ToArray();
    }

    public void RunFromPath<TEnv>(string path, ref TEnv env)
        where TEnv : IRunVirtualEnvironmentProperty, IInputStateProperty
    {
        // TODO load text from path
        var pathText = path;

        // parse
        var movie = _parser.Parse(pathText);

        // warnings

        // TODO apply environment

        // init engine
        InitEngine(movie.Script);

        // set env
        env.InputState.ResetStates();
        env.RunVirtualEnvironment = true;
        // TODO other stuff like save state load, reset, hide cursor, etc
        // TODO handle empty movie

        IsRunning = true;
        throw new NotImplementedException();
    }

    private void InitEngine(ScriptModel script)
    {
        _registers = new Register[Enum.GetNames(typeof(RegisterType)).Length];
        MovieEnd = false;
        _pc = 0;
        _currentMethod = -1;

        _mainMethod = script.MainMethod.OpCodes;
        _methods = script.Methods.ToArray();
    }

    private void ProcessUntilStop()
    {
        var opCodes = _currentMethod < 0 ? _mainMethod : _methods[_currentMethod].OpCodes;

        while (true)
        {
            var opCode = opCodes[_pc];

            switch (opCode)
            {
                case BitwiseAndOpCode bitwiseAndOpCode:
                {
                    var and = (_registers[(int)bitwiseAndOpCode.Left].InnerValue as IntValueType).Value &
                              (_registers[(int)bitwiseAndOpCode.Right].InnerValue as IntValueType).Value;
                    _registers[(int)bitwiseAndOpCode.ResultRegister].InnerValue = new IntValueType(and);
                    _pc++;
                    break;
                }
                case BitwiseOrOpCode bitwiseOrOpCode:
                {
                    var or = (_registers[(int)bitwiseOrOpCode.Left].InnerValue as IntValueType).Value &
                             (_registers[(int)bitwiseOrOpCode.Right].InnerValue as IntValueType).Value;
                    _registers[(int)bitwiseOrOpCode.ResultRegister].InnerValue = new IntValueType(or);
                    _pc++;
                    break;
                }
                case BitwiseShiftLeftOpCode bitwiseShiftLeftOpCode:
                {
                    var shiftLeft = (_registers[(int)bitwiseShiftLeftOpCode.Left].InnerValue as IntValueType).Value <<
                                    (_registers[(int)bitwiseShiftLeftOpCode.Right].InnerValue as IntValueType).Value;
                    _registers[(int)bitwiseShiftLeftOpCode.ResultRegister].InnerValue = new IntValueType(shiftLeft);
                    _pc++;
                    break;
                }
                case BitwiseShiftRightOpCode bitwiseShiftRightOpCode:
                {
                    var shiftRight = (_registers[(int)bitwiseShiftRightOpCode.Left].InnerValue as IntValueType).Value >>
                                     (_registers[(int)bitwiseShiftRightOpCode.Right].InnerValue as IntValueType).Value;
                    _registers[(int)bitwiseShiftRightOpCode.ResultRegister].InnerValue = new IntValueType(shiftRight);
                    _pc++;
                    break;
                }
                case BitwiseXorOpCode bitwiseXorOpCode:
                {
                    var xor = (_registers[(int)bitwiseXorOpCode.Left].InnerValue as IntValueType).Value ^
                              (_registers[(int)bitwiseXorOpCode.Right].InnerValue as IntValueType).Value;
                    _registers[(int)bitwiseXorOpCode.ResultRegister].InnerValue = new IntValueType(xor);
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
                        _ => throw new NotImplementedException()
                    };
                    break;
                }
                case FrameAdvanceOpCode frameAdvanceOpCode:
                    break;
                case JumpIfEqOpCode jumpIfEqOpCode:
                    break;
                case JumpIfEqZero jumpIfEqZero:
                    break;
                case JumpIfFalse jumpIfFalse:
                    break;
                case JumpIfGtEqOpCode jumpIfGtEqOpCode:
                    break;
                case JumpIfGtOpCode jumpIfGtOpCode:
                    break;
                case JumpIfLtEqOpCode jumpIfLtEqOpCode:
                    break;
                case JumpIfLtOpCode jumpIfLtOpCode:
                    break;
                case JumpIfNEqOpCode jumpIfNEqOpCode:
                    break;
                case JumpIfTrue jumpIfTrue:
                    break;
                case JumpOpCode jumpOpCode:
                    break;
                case AndOpCode andOpCode:
                    break;
                case EqualOpCode equalOpCode:
                    break;
                case GreaterEqualOpCode greaterEqualOpCode:
                    break;
                case GreaterOpCode greaterOpCode:
                    break;
                case LessEqualOpCode lessEqualOpCode:
                    break;
                case LessOpCode lessOpCode:
                    break;
                case NotEqualOpCode notEqualOpCode:
                    break;
                case NotOpCode notOpCode:
                    break;
                case OrOpCode orOpCode:
                    break;
                case BreakOpCode breakOpCode:
                    break;
                case ContinueOpCode continueOpCode:
                    break;
                case AddOpCode addOpCode:
                    break;
                case DivOpCode divOpCode:
                    break;
                case FlipNegativeOpCode flipNegativeOpCode:
                    break;
                case ModOpCode modOpCode:
                    break;
                case MultOpCode multOpCode:
                    break;
                case SubOpCode subOpCode:
                    break;
                case GotoMethodOpCode gotoMethodOpCode:
                    break;
                case PopArgOpCode popArgOpCode:
                    break;
                case PushArgOpCode pushArgOpCode:
                    break;
                case ReturnOpCode returnOpCode:
                    break;
                case PushListOpCode pushListOpCode:
                    break;
                case ConstToRegisterOpCode constToRegisterOpCode:
                    break;
                case MoveOpCode moveOpCode:
                    break;
                case VarToRegisterOpCode varToRegisterOpCode:
                    break;
                case EnterScopeOpCode enterScopeOpCode:
                    break;
                case ExitScopeOpCode exitScopeOpCode:
                    break;
                case SetVariableOpCode setVariableOpCode:
                    break;
                case PopStackOpCode popStackOpCode:
                    break;
                case PushStackOpCode pushStackOpCode:
                    break;
                case PopTupleOpCode popTupleOpCode:
                    break;
                case PushTupleOpCode pushTupleOpCode:
                    break;
                case BitwiseBase bitwiseBase:
                    break;
                case JumpCompareBase jumpCompareBase:
                    break;
                case LogicComparisonBase logicComparisonBase:
                    break;
                case MathOpBase mathOpBase:
                    break;
                case RegisterSetBase registerSetBase:
                    break;
                case JumpBase jumpBase:
                    break;
                case LogicBase logicBase:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(opCode));
            }
        }
    }

    public void Update<TEnv>(ref TEnv env)
        where TEnv :
        IRunVirtualEnvironmentProperty,
        IInputStateProperty
    {
        if (!IsRunning)
            return;
        // TODO input handle
        /*MouseState.Position = new Vector2(fb.Mouse.X, fb.Mouse.Y);
        MouseState.LeftClick = fb.Mouse.Left;
        MouseState.RightClick = fb.Mouse.Right;
        MouseState.MiddleClick = fb.Mouse.Middle;

        List<string> axisMoveSetDefault = new();
        foreach (var pair in AxisState.Values)
        {
            var key = pair.Key;
            if (!fb.Axises.AxisMove.ContainsKey(key))
                axisMoveSetDefault.Add(key);
        }
        foreach (var key in axisMoveSetDefault)
        {
            if (AxisState.Values.ContainsKey(key))
                AxisState.Values[key] = default;
            else
                AxisState.Values.Add(key, default);
        }
        foreach (var axisValue in fb.Axises.AxisMove)
        {
            var axis = axisValue.Key;
            var value = axisValue.Value;
            if (AxisState.Values.ContainsKey(axis))
            {
                AxisState.Values[axis] = value;
            }
            else
            {
                AxisState.Values.Add(axis, value);
            }
        }*/

        /*
        if (_scriptEngine.MovieEnd)
        {
            IsRunning = false;
            AtMovieEnd(ref env);
            return;
        }

        // TODO
        _scriptEngine.AdvanceFrame();
        */

        throw new NotImplementedException();
    }

    private void AtMovieEnd<TEnv>(ref TEnv env)
        where TEnv :
        IRunVirtualEnvironmentProperty
    {
        env.RunVirtualEnvironment = false;
        // TODO set frameTime to 0
        throw new NotImplementedException();
    }

    public void AdvanceFrame()
    {
        throw new NotImplementedException();
    }
}