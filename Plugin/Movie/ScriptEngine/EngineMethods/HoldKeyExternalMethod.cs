using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods;

public class HoldKeyExternalMethod : EngineExternalMethod
{
    public HoldKeyExternalMethod() : base("hold_key", 1)
    {
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, ScriptEngineMovieRunner runner)
    {
        var arg = args.First();
        var keyCodeArg = arg.First();
        if (keyCodeArg is not IntValueType keyCode) return new();
        
        runner.UpdateHeldKey(true, keyCode.Value);
        
        return new();
    }
}