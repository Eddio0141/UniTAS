using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.EngineMethods;

public class MoveAxisExternalMethod : EngineExternalMethod
{
    private readonly VirtualEnvironment _virtualEnvironment;

    public MoveAxisExternalMethod(VirtualEnvironment virtualEnvironmentFactory) : base("move_axis", 2)
    {
        _virtualEnvironment = virtualEnvironmentFactory;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
    {
        var argsList = args.ToList();
        var axisArg = argsList[0].First();
        if (axisArg is not StringValueType axis) return new();

        var valueArg = argsList[1].First();
        if (valueArg is not FloatValueType value) return new();

        _virtualEnvironment.InputState.AxisState.Values[axis.Value] = value.Value;

        return new();
    }
}