using System.Collections.Generic;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.EngineMethods;

public class GetFrameTimeExternalMethod : EngineExternalMethod
{
    private readonly IVirtualEnvironmentFactory _virtualEnvironmentFactory;

    public GetFrameTimeExternalMethod(IVirtualEnvironmentFactory virtualEnvironmentFactory) : base("get_frametime", 0,
        1)
    {
        _virtualEnvironmentFactory = virtualEnvironmentFactory;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, ScriptEngineMovieRunner runner)
    {
        var frameTime = _virtualEnvironmentFactory.GetVirtualEnv().FrameTime;

        return new() { new FloatValueType(frameTime) };
    }
}