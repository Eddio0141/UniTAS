using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods;

public class MoveMouseExternalMethod : EngineExternalMethod
{
    private readonly IVirtualEnvironmentService _virtualEnvironmentService;

    public MoveMouseExternalMethod(IVirtualEnvironmentService virtualEnvironmentService) : base("move_mouse", 2)
    {
        _virtualEnvironmentService = virtualEnvironmentService;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, ScriptEngineMovieRunner runner)
    {
        var argsList = args.ToList();
        var xArg = argsList[0].First();
        if (xArg is not IntValueType x) return new();

        var yArg = argsList[0].First();
        if (yArg is not IntValueType y) return new();

        var env = _virtualEnvironmentService.GetVirtualEnv();
        env.InputState.MouseState.XPos = x.Value;
        env.InputState.MouseState.YPos = y.Value;

        return new();
    }
}