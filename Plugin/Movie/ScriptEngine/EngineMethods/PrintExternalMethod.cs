using System.Collections.Generic;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods;

public class PrintExternalMethod : EngineExternalMethodBase
{
    public PrintExternalMethod() : base("print", -1)
    {
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args)
    {
        // TODO add more way of logging
        foreach (var arg in args)
        {
            Plugin.Log.LogInfo(arg.ToString());
        }

        return new();
    }
}