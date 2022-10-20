using System;
using System.Collections.Generic;
using BepInEx;
using UniTASPlugin.Movie.Exceptions.ScriptEngineExceptions;
using UniTASPlugin.Movie.Models.Script;
using UniTASPlugin.Movie.ScriptEngine.OpCodes;

namespace UniTASPlugin.Movie.ScriptEngine;

public class MovieScriptEngine
{
    private Register[] _registers;
    private OpCodeBase[] _mainMethod;
    private readonly Dictionary<string, OpCodeBase[]> _methods;
    public bool MovieEnd { get; private set; }

    public MovieScriptEngine(ScriptModel script)
    {
        _registers = new Register[Enum.GetNames(typeof(RegisterType)).Length];

        _mainMethod = script.MainMethod.OpCodes;
        _methods = new Dictionary<string, OpCodeBase[]>();
        foreach (var scriptMethodModel in script.Methods)
        {
            _methods.Add(scriptMethodModel.Name, scriptMethodModel.OpCodes);
        }

        MovieEnd = false;
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