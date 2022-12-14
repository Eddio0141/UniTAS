using System;
using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;
using ValueType = UniTASPlugin.Movie.ScriptEngine.ValueTypes.ValueType;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods;

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
        if (keyCodeArg is not StringValueType keyCodeRaw) return new();
        if (!Enum.IsDefined(typeof(UnityEngine.KeyCode), keyCodeRaw.Value)) return new();
        var keyCode = (UnityEngine.KeyCode)Enum.Parse(typeof(UnityEngine.KeyCode), keyCodeRaw.Value);

        _virtualEnvironmentService.GetVirtualEnv().InputState.KeyboardState.Keys
            .RemoveAt((int)keyCode);

        return new();
    }
}