using System;
using System.Collections.Generic;
using BepInEx;
using UniTASPlugin.Movie.Script;
using UniTASPlugin.MovieEngine.OpCodes;

namespace UniTASPlugin.MovieEngine;

public class MovieEngine
{
    private Register[] _registers;
    private OpCodeBase[] _mainMethod;
    private Dictionary<string, OpCodeBase[]> _methods;

    public MovieEngine(ScriptModel script)
    {
        _registers = new Register[Enum.GetNames(typeof(RegisterType)).Length];

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
}