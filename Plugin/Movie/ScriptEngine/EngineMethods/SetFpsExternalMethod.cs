using System;
using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;
using ValueType = UniTASPlugin.Movie.ScriptEngine.ValueTypes.ValueType;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods;

public class SetFpsExternalMethod : EngineExternalMethod
{
    private readonly IVirtualEnvironmentService _virtualEnvironmentService;

    public SetFpsExternalMethod(IVirtualEnvironmentService virtualEnvironmentService) : base("set_fps", 1)
    {
        _virtualEnvironmentService = virtualEnvironmentService;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, ScriptEngineMovieRunner runner)
    {
        var fpsArg = args.First().First();

        float fps;
        switch (fpsArg)
        {
            case IntValueType fpsInt:
                fps = fpsInt.Value;
                break;
            case FloatValueType fpsFloat:
                fps = fpsFloat.Value;
                break;
            default:
                return new();
        }

        _virtualEnvironmentService.GetVirtualEnv().FrameTime = 1 / fps;

        return new();
    }
}