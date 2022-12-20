using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.EngineMethods;

public class UnregisterExternalMethod : EngineExternalMethod
{
    public UnregisterExternalMethod() : base("unregister", 2)
    {
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
    {
        var argsList = args.ToList();

        var hashCodeArg = argsList[0].First();
        if (hashCodeArg is not IntValueType hashCode)
        {
            return new();
        }

        var preUpdateArg = argsList[1].First();
        if (preUpdateArg is not BoolValueType preUpdate)
        {
            return new();
        }

        runner.UnregisterConcurrentMethod(hashCode.Value, preUpdate.Value);
        return new();
    }
}