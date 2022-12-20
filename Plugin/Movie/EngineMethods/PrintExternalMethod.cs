using System.Collections.Generic;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.EngineMethods;

public class PrintExternalMethod : EngineExternalMethod
{
    public PrintExternalMethod() : base("print", -1)
    {
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
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