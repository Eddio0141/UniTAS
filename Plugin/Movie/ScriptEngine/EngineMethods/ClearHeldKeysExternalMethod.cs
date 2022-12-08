using System.Collections.Generic;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods.Exceptions;

public class ClearHeldKeysExternalMethod : EngineExternalMethod
{
    public ClearHeldKeysExternalMethod() : base("clear_held_keys")
    {
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, ScriptEngineMovieRunner runner)
    {
        runner.ClearHeldKeys();
        return new();
    }
}