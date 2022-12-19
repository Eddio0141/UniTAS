using System.Collections.Generic;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.EngineMethods;

public class GetFpsExternalMethod : EngineExternalMethod
{
    private readonly IVirtualEnvironmentFactory _virtualEnvironmentFactory;

    public GetFpsExternalMethod(IVirtualEnvironmentFactory virtualEnvironmentFactory) : base("get_fps", 0, 1)
    {
        _virtualEnvironmentFactory = virtualEnvironmentFactory;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
    {
        var frameTime = _virtualEnvironmentFactory.GetVirtualEnv().FrameTime;

        return new() { new FloatValueType(1 / frameTime) };
    }
}