using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods;

public class HoldKeyExternalMethod : EngineExternalMethod
{
    private readonly IVirtualEnvironmentService _virtualEnvironmentService;

    public HoldKeyExternalMethod(IVirtualEnvironmentService virtualEnvironmentService) : base("hold_key", 1)
    {
        _virtualEnvironmentService = virtualEnvironmentService;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, ScriptEngineMovieRunner runner)
    {
        var arg = args.First();
        var keyCodeArg = arg.First();
        if (keyCodeArg is not IntValueType keyCode) return new();

        _virtualEnvironmentService.GetVirtualEnv().InputState.KeyboardState.Keys.Add(keyCode.Value);

        return new();
    }
}