using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ValueTypes;
using ValueType = UniTASPlugin.Movie.ValueTypes.ValueType;

namespace UniTASPlugin.Movie.EngineMethods;

public class SetFpsExternalMethod : EngineExternalMethod
{
    private readonly VirtualEnvironment _virtualEnvironment;

    public SetFpsExternalMethod(VirtualEnvironment virtualEnvironmentFactory) : base("set_fps", 1)
    {
        _virtualEnvironment = virtualEnvironmentFactory;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
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

        _virtualEnvironment.FrameTime = 1 / fps;

        return new();
    }
}