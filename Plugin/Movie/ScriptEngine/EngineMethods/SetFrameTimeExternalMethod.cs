using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods;

public class SetFrameTimeExternalMethod : EngineExternalMethod
{
    private readonly IVirtualEnvironmentService _virtualEnvironmentService;

    public SetFrameTimeExternalMethod(IVirtualEnvironmentService virtualEnvironmentService) : base("set_frametime", 1)
    {
        _virtualEnvironmentService = virtualEnvironmentService;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, ScriptEngineMovieRunner runner)
    {
        var frameTimeArg = args.First().First();

        if (frameTimeArg is not FloatValueType frameTime) return new();

        _virtualEnvironmentService.GetVirtualEnv().FrameTime = frameTime.Value;

        return new();
    }
}