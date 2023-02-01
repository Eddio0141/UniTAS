using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.EngineMethods;

public class MoveAxisExternalMethod : EngineExternalMethod
{
    private readonly IVirtualEnvironmentFactory _virtualEnvironmentFactory;

    public MoveAxisExternalMethod(IVirtualEnvironmentFactory virtualEnvironmentFactory) : base("move_axis", 2)
    {
        _virtualEnvironmentFactory = virtualEnvironmentFactory;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
    {
        var argsList = args.ToList();
        var axisArg = argsList[0].First();
        if (axisArg is not StringValueType axis) return new();

        var valueArg = argsList[1].First();
        if (valueArg is not FloatValueType value) return new();

        var env = _virtualEnvironmentFactory.GetVirtualEnv();
        env.InputState.AxisState.Values[axis.Value] = value.Value;

        return new();
    }
}