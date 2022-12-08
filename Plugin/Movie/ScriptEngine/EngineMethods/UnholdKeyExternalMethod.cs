using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods.Exceptions;

public class UnHoldKeyExternalMethod : EngineExternalMethod
{
    // ReSharper disable once StringLiteralTypo
    public UnHoldKeyExternalMethod() : base("unhold_key", 1)
    {
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, ScriptEngineMovieRunner runner)
    {
        var arg = args.First();
        var keyCodeArg = arg.First();
        if (keyCodeArg is not IntValueType keyCode) return new();

        runner.UpdateHeldKey(false, keyCode.Value);

        return new();
    }
}