using System;
using System.Collections.Generic;
using BepInEx;
using UniTASPlugin.Movie.Exceptions.ScriptEngineExceptions;
using UniTASPlugin.Movie.Models.Script;
using UniTASPlugin.Movie.ScriptEngine.EngineInterfaces;
using UniTASPlugin.Movie.ScriptEngine.OpCodes;

namespace UniTASPlugin.Movie.ScriptEngine;

public class MovieScriptEngine :
    IScriptEngineInitScript,
    IScriptEngineMovieEnd,
    IScriptEngineCurrentState,
    IScriptEngineAdvanceFrame,
    IScriptEngineAddMethod
{
    private Register[] _registers;
    private OpCodeBase[] _mainMethod;
    private Dictionary<string, OpCodeBase[]> _methods;
    public bool MovieEnd { get; private set; }

    public MovieScriptEngine()
    {
        _registers = new Register[Enum.GetNames(typeof(RegisterType)).Length];
        MovieEnd = false;
    }

    public void Init(ScriptModel script)
    {
        _mainMethod = script.MainMethod.OpCodes;
        _methods = new Dictionary<string, OpCodeBase[]>();
        foreach (var scriptMethodModel in script.Methods)
        {
            _methods.Add(scriptMethodModel.Name, scriptMethodModel.OpCodes);
        }
    }

    public void AddMethod(ScriptMethodModel method)
    {
        if (_methods.ContainsKey(method.Name) || method.Name.IsNullOrWhiteSpace())
        {
            throw new MovieMethodAlreadyDefinedException();
        }
        _methods.Add(method.Name, method.OpCodes);
    }

    public void CurrentState()
    {
        throw new NotImplementedException();
    }

    public void AdvanceFrame()
    {
        throw new NotImplementedException();
    }
}