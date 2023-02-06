using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.EngineMethods;

public class SetFrameTimeExternalMethod : EngineExternalMethod
{
    private readonly VirtualEnvironment _virtualEnvironment;

    public SetFrameTimeExternalMethod(VirtualEnvironment virtualEnvironmentFactory) : base("set_frametime", 1)
    {
        _virtualEnvironment = virtualEnvironmentFactory;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
    {
        var frameTimeArg = args.First().First();

        if (frameTimeArg is not FloatValueType frameTime) return new();

        _virtualEnvironment.FrameTime = frameTime.Value;

        return new();
    }
}