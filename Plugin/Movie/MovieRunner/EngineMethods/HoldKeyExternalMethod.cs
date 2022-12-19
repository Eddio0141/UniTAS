using System;
using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.MovieRunner.ValueTypes;
using ValueType = UniTASPlugin.Movie.MovieRunner.ValueTypes.ValueType;

namespace UniTASPlugin.Movie.MovieRunner.EngineMethods;

public class HoldKeyExternalMethod : EngineExternalMethod
{
    private readonly IVirtualEnvironmentFactory _virtualEnvironmentFactory;

    public HoldKeyExternalMethod(IVirtualEnvironmentFactory virtualEnvironmentFactory) : base("hold_key", 1)
    {
        _virtualEnvironmentFactory = virtualEnvironmentFactory;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, ScriptEngineMovieRunner runner)
    {
        var arg = args.First();
        var keyCodeArg = arg.First();
        if (keyCodeArg is not StringValueType keyCodeRaw) return new();
        if (!Enum.IsDefined(typeof(UnityEngine.KeyCode), keyCodeRaw.Value)) return new();
        var keyCode = (UnityEngine.KeyCode)Enum.Parse(typeof(UnityEngine.KeyCode), keyCodeRaw.Value);

        _virtualEnvironmentFactory.GetVirtualEnv().InputState.KeyboardState.Keys.Add((int)keyCode);

        return new();
    }
}