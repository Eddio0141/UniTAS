using System;
using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ValueTypes;
using ValueType = UniTASPlugin.Movie.ValueTypes.ValueType;

namespace UniTASPlugin.Movie.EngineMethods;

public class ReleaseKeyExternalMethod : EngineExternalMethod
{
    private readonly VirtualEnvironment _virtualEnvironment;

    public ReleaseKeyExternalMethod(VirtualEnvironment virtualEnvironmentFactory) : base("release_key", 1)
    {
        _virtualEnvironment = virtualEnvironmentFactory;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
    {
        var arg = args.First();
        var keyCodeArg = arg.First();
        if (keyCodeArg is not StringValueType keyCodeRaw) return new();
        if (!Enum.IsDefined(typeof(UnityEngine.KeyCode), keyCodeRaw.Value)) return new();
        var keyCode = (UnityEngine.KeyCode)Enum.Parse(typeof(UnityEngine.KeyCode), keyCodeRaw.Value);

        _virtualEnvironment.InputState.KeyboardState.Keys
            .Remove((int)keyCode);

        return new();
    }
}