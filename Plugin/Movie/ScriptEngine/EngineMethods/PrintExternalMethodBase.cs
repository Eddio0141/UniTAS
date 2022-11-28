using System.Collections.Generic;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods;

public class PrintExternalMethodBase : EngineExternalMethodBase
{
    public PrintExternalMethodBase() : base("print", -1)
    {
    }

    public override ValueType Invoke(ICollection<ValueType> args)
    {
        // TODO add more way of logging
        foreach (var arg in args)
        {
            Plugin.Log.LogInfo(arg.ToString());
        }

        return null;
    }
}