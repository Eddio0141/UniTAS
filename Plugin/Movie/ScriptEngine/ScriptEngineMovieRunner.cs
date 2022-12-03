﻿using System;
using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment.Interfaces;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods.Exceptions;
using UniTASPlugin.Movie.ScriptEngine.LowLevelEngine;
using UniTASPlugin.Movie.ScriptEngine.MovieModels.Script;
using UniTASPlugin.Movie.ScriptEngine.OpCodes;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Method;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.RegisterSet;
using UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;
using ValueType = UniTASPlugin.Movie.ScriptEngine.ValueTypes.ValueType;

namespace UniTASPlugin.Movie.ScriptEngine;

public class ScriptEngineMovieRunner : IMovieRunner
{
    public bool MovieEnd { get; private set; }
    public bool IsRunning => !MovieEnd;

    private readonly IMovieParser _parser;
    private readonly EngineExternalMethod[] _externalMethods;

    private ScriptEngineLowLevelEngine _engine;
    private ScriptModel _mainScript;
    private readonly List<ScriptEngineLowLevelEngine> _concurrentRunnersPreUpdate = new();
    private readonly List<ScriptEngineLowLevelEngine> _concurrentRunnersPostUpdate = new();

    public ScriptEngineMovieRunner(IMovieParser parser, IEnumerable<EngineExternalMethod> externMethods)
    {
        _parser = parser;
        _externalMethods = externMethods.ToArray();
    }

    public void RunFromPath<TEnv>(string path, ref TEnv env)
        where TEnv : IRunVirtualEnvironmentProperty, IInputStateProperty
    {
        // TODO load text from path
        var pathText = path;

        // parse
        var movie = _parser.Parse(pathText);
        _mainScript = movie.Script;

        // warnings

        // TODO apply environment

        // init engine
        _engine = new ScriptEngineLowLevelEngine(_mainScript, _externalMethods);

        // set env
        env.InputState.ResetStates();
        env.RunVirtualEnvironment = true;
        // TODO other stuff like save state load, reset, hide cursor, etc
        // TODO handle empty movie

        MovieEnd = false;

        // TODO do we advance frame at the start?
        _engine.ExecUntilStop();
        throw new NotImplementedException();
    }

    public void Update<TEnv>(ref TEnv env)
        where TEnv :
        IRunVirtualEnvironmentProperty,
        IInputStateProperty
    {
        if (!IsRunning)
            return;

        ConcurrentRunnersPreUpdate();
        _engine.ExecUntilStop();
        ConcurrentRunnersPostUpdate();

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

        if (_engine.FinishedExecuting)
        {
            MovieEnd = true;
            AtMovieEnd(ref env);
            return;
        }

        _engine.ExecUntilStop();

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

    public void RegisterConcurrentMethod(string methodName, bool preUpdate,
        IEnumerable<IEnumerable<ValueType>> defaultArgs)
    {
        if (methodName == null) return;

        var foundDefinedMethod = _mainScript.Methods.ToList().Find(x => x.Name == methodName);
        var externFound = foundDefinedMethod == null ? _externalMethods.ToList().Find(x => x.Name == methodName) : null;

        if (foundDefinedMethod == null && externFound == null)
        {
            return;
        }

        // check if arg count match
        var argCount = foundDefinedMethod == null
            ? externFound.ArgCount
            : foundDefinedMethod.OpCodes.Count(x => x is PopArgOpCode);

        var argTuples = defaultArgs as IEnumerable<ValueType>[] ?? defaultArgs.ToArray();
        if (argCount != argTuples.Count())
        {
            throw new MethodArgCountNotMatching(argCount.ToString(), argTuples.Count().ToString());
        }

        ScriptModel runnerScript;
        if (foundDefinedMethod != null)
        {
            var mainMethod = new List<OpCodeBase>();
            foreach (var argTuple in argTuples)
            {
                foreach (var arg in argTuple)
                {
                    mainMethod.Add(new ConstToRegisterOpCode(RegisterType.Temp0, arg));
                    mainMethod.Add(new PushArgOpCode(RegisterType.Temp0));
                }
            }

            runnerScript = new ScriptModel(new(null, mainMethod),
                new[] { foundDefinedMethod });
        }
        else
        {
            var wrapperMethod = new List<OpCodeBase>();
            foreach (var argTuple in argTuples)
            {
                foreach (var arg in argTuple)
                {
                    wrapperMethod.Add(new ConstToRegisterOpCode(RegisterType.Temp0, arg));
                    wrapperMethod.Add(new PushArgOpCode(RegisterType.Temp0));
                }
            }

            wrapperMethod.Add(new GotoMethodOpCode(externFound.Name));
            runnerScript = new ScriptModel(new ScriptMethodModel(null, wrapperMethod), new ScriptMethodModel[0]);
        }

        var engine = new ScriptEngineLowLevelEngine(runnerScript, _externalMethods);

        if (preUpdate)
        {
            _concurrentRunnersPreUpdate.Add(engine);
        }
        else
        {
            _concurrentRunnersPostUpdate.Add(engine);
        }
    }

    private void ConcurrentRunnersPreUpdate()
    {
        foreach (var runner in _concurrentRunnersPreUpdate)
        {
            runner.ExecUntilStop();
        }
    }

    private void ConcurrentRunnersPostUpdate()
    {
        foreach (var runner in _concurrentRunnersPostUpdate)
        {
            runner.ExecUntilStop();
        }
    }
}