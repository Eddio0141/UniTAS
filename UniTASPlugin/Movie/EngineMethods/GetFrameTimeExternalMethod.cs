using System.Collections.Generic;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.EngineMethods;

public class GetFrameTimeExternalMethod : EngineExternalMethod
{
    private readonly VirtualEnvironment _virtualEnvironment;

    public GetFrameTimeExternalMethod(VirtualEnvironment virtualEnvironmentFactory) : base("get_frametime", 0,
        1)
    {
        _virtualEnvironment = virtualEnvironmentFactory;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
    {
        var frameTime = _virtualEnvironment.FrameTime;

        return new() { new FloatValueType(frameTime) };
    }
}