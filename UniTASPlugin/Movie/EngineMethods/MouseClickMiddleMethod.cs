using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.EngineMethods;

public class MouseClickMiddleMethod : EngineExternalMethod
{
    private readonly VirtualEnvironment _virtualEnvironment;

    public MouseClickMiddleMethod(VirtualEnvironment virtualEnvironmentFactory) : base("middle_click", 1)
    {
        _virtualEnvironment = virtualEnvironmentFactory;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
    {
        var arg = args.First().First();
        if (arg is not BoolValueType boolValue) return new();

        _virtualEnvironment.InputState.MouseState.LeftClick = boolValue.Value;

        return new();
    }
}