using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.EngineMethods;

public class MoveMouseExternalMethod : EngineExternalMethod
{
    private readonly IVirtualEnvironmentFactory _virtualEnvironmentFactory;

    public MoveMouseExternalMethod(IVirtualEnvironmentFactory virtualEnvironmentFactory) : base("move_mouse", 2)
    {
        _virtualEnvironmentFactory = virtualEnvironmentFactory;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, ScriptEngineMovieRunner runner)
    {
        var argsList = args.ToList();
        var xArg = argsList[0].First();
        if (xArg is not IntValueType x) return new();

        var yArg = argsList[0].First();
        if (yArg is not IntValueType y) return new();

        var env = _virtualEnvironmentFactory.GetVirtualEnv();
        env.InputState.MouseState.XPos = x.Value;
        env.InputState.MouseState.YPos = y.Value;

        return new();
    }
}