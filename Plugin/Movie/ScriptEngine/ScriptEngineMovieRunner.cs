using System;
using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment.Interfaces;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods;
using UniTASPlugin.Movie.ScriptEngine.MovieModels.Script;
using UniTASPlugin.Movie.ScriptEngine.OpCodes;
using UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;

namespace UniTASPlugin.Movie.ScriptEngine;

public class ScriptEngineMovieRunner : IMovieRunner
{
    private Register[] _registers;
    private OpCodeBase[] _mainMethod;
    private Dictionary<string, OpCodeBase[]> _methods;
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

        _mainMethod = script.MainMethod.OpCodes;
        _methods = new Dictionary<string, OpCodeBase[]>();
        foreach (var scriptMethodModel in script.Methods)
        {
            _methods.Add(scriptMethodModel.Name, scriptMethodModel.OpCodes);
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