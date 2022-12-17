using System.Collections.Generic;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods;

public class PrintExternalMethod : EngineExternalMethod
{
    public PrintExternalMethod() : base("print", -1)
    {
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, ScriptEngineMovieRunner runner)
    {
        // TODO add more way of logging
        foreach (var arg in args)
        {
            var values = new List<string>();
            foreach (var value in arg)
            {
                values.Add(value.ToString());
            }

            Plugin.Log.LogInfo(string.Join(", ", values.ToArray()));
        }

        return new();
    }
}