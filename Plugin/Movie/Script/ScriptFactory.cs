using System;
using System.Collections.Generic;
using UniTASPlugin.Movie.DefaultParser;

namespace UniTASPlugin.Movie.Script;

public class ScriptFactory
{
    public static ScriptModel ParseFromText(string input)
    {
        var scriptParser = new DefaultMovieScriptParser();
        var scriptParsed = scriptParser.Parse(input);

        var mainMethod = new ScriptMethodModel();
        var definedMethods = new List<ScriptMethodModel>();

        foreach (var methodNameCode in scriptParsed)
        {
            var methodName = methodNameCode.Key;
            var opCodes = methodNameCode.Value;
            var method = new ScriptMethodModel(methodName, opCodes);

            if (methodName != null)
            {
                mainMethod = method;
            }
            else
            {
                definedMethods.Add(method);
            }
        }

        return new ScriptModel(mainMethod, definedMethods);
    }
}