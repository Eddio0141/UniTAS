using System.Collections.Generic;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods;

public class GetFrameTimeExternalMethod : EngineExternalMethod
{
    private readonly IVirtualEnvironmentService _virtualEnvironmentService;

    public GetFrameTimeExternalMethod(IVirtualEnvironmentService virtualEnvironmentService) : base("get_frametime", 0,
        1)
    {
        _virtualEnvironmentService = virtualEnvironmentService;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, ScriptEngineMovieRunner runner)
    {
        var frameTime = _virtualEnvironmentService.GetVirtualEnv().FrameTime;

        return new() { new FloatValueType(frameTime) };
    }
}