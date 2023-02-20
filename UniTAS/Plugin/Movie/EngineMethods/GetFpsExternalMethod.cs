using System.Collections.Generic;
using UniTAS.Plugin.GameEnvironment;
using UniTAS.Plugin.Movie.ValueTypes;

namespace UniTAS.Plugin.Movie.EngineMethods;

public class GetFpsExternalMethod : EngineExternalMethod
{
    private readonly VirtualEnvironment _virtualEnvironment;

    public GetFpsExternalMethod(VirtualEnvironment virtualEnvironmentFactory) : base("get_fps", 0, 1)
    {
        _virtualEnvironment = virtualEnvironmentFactory;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
    {
        var frameTime = _virtualEnvironment.FrameTime;

        return new() { new FloatValueType(1 / frameTime) };
    }
}