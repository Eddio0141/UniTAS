using System.Collections.Generic;
using System.Linq;
using UniTAS.Plugin.GameEnvironment;
using UniTAS.Plugin.Movie.ValueTypes;

namespace UniTAS.Plugin.Movie.EngineMethods;

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
        var axisValue = valueArg switch
        {
            IntValueType intVal => intVal.Value,
            FloatValueType floatVal => floatVal.Value,
            _ => 0
        };

        _virtualEnvironment.InputState.AxisState.Values[axis.Value] = axisValue;

        return new();
    }
}