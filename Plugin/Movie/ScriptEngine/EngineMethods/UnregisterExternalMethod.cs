using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods;

public class UnregisterExternalMethod : EngineExternalMethod
{
    public UnregisterExternalMethod() : base("unregister", 2)
    {
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, ScriptEngineMovieRunner runner)
    {
        var argsList = args.ToList();

        var indexArg = argsList[0].First();
        if (indexArg is not IntValueType index)
        {
            return new();
        }

        var preUpdateArg = argsList[1].First();
        if (preUpdateArg is not BoolValueType preUpdate)
        {
            return new();
        }

        runner.UnregisterConcurrentMethod(index.Value, preUpdate.Value);
        return new();
    }
}