using System;
using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ValueTypes;
using ValueType = UniTASPlugin.Movie.ValueTypes.ValueType;

namespace UniTASPlugin.Movie.EngineMethods;

public class ReleaseKeyExternalMethod : EngineExternalMethod
{
    private readonly IVirtualEnvironmentFactory _virtualEnvironmentFactory;

    public ReleaseKeyExternalMethod(IVirtualEnvironmentFactory virtualEnvironmentFactory) : base("release_key", 1)
    {
        _virtualEnvironmentFactory = virtualEnvironmentFactory;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
    {
        var arg = args.First();
        var keyCodeArg = arg.First();
        if (keyCodeArg is not StringValueType keyCodeRaw) return new();
        if (!Enum.IsDefined(typeof(UnityEngine.KeyCode), keyCodeRaw.Value)) return new();
        var keyCode = (UnityEngine.KeyCode)Enum.Parse(typeof(UnityEngine.KeyCode), keyCodeRaw.Value);

        _virtualEnvironmentFactory.GetVirtualEnv().InputState.KeyboardState.Keys
            .Remove((int)keyCode);

        return new();
    }
}