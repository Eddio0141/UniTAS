using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods.Exceptions;

public class UnHoldKeyExternalMethod : EngineExternalMethod
{
    private readonly IVirtualEnvironmentService _virtualEnvironmentService;

    // ReSharper disable once StringLiteralTypo
    public UnHoldKeyExternalMethod(IVirtualEnvironmentService virtualEnvironmentService) : base("unhold_key", 1)
    {
        _virtualEnvironmentService = virtualEnvironmentService;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, ScriptEngineMovieRunner runner)
    {
        var arg = args.First();
        var keyCodeArg = arg.First();
        if (keyCodeArg is not IntValueType keyCode) return new();

        _virtualEnvironmentService.GetVirtualEnv().InputState.KeyboardState.Keys.RemoveAt(keyCode.Value);

        return new();
    }
}