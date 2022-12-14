using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.EngineMethods;

public class SetFrameTimeExternalMethod : EngineExternalMethod
{
    private readonly IVirtualEnvironmentFactory _virtualEnvironmentFactory;

    public SetFrameTimeExternalMethod(IVirtualEnvironmentFactory virtualEnvironmentFactory) : base("set_frametime", 1)
    {
        _virtualEnvironmentFactory = virtualEnvironmentFactory;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
    {
        var frameTimeArg = args.First().First();

        if (frameTimeArg is not FloatValueType frameTime) return new();

        _virtualEnvironmentFactory.GetVirtualEnv().FrameTime = frameTime.Value;

        return new();
    }
}