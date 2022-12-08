using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods;

public class MoveMouseExternalMethod : EngineExternalMethod
{
    public MoveMouseExternalMethod() : base("move_mouse", 2)
    {
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, ScriptEngineMovieRunner runner)
    {
        var argsList = args.ToList();
        var xArg = argsList[0].First();
        if (xArg is not IntValueType x) return new();

        var yArg = argsList[0].First();
        if (yArg is not IntValueType y) return new();
        
        runner.MoveMouse(x.Value, y.Value);

        return new();
    }
}