using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.EngineMethods;

public class MoveMouseExternalMethod : EngineExternalMethod
{
    private readonly VirtualEnvironment _virtualEnvironment;

    public MoveMouseExternalMethod(VirtualEnvironment virtualEnvironmentFactory) : base("move_mouse", 2)
    {
        _virtualEnvironment = virtualEnvironmentFactory;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
    {
        var argsList = args.ToList();
        var xArg = argsList[0].First();
        if (xArg is not IntValueType x) return new();

        var yArg = argsList[1].First();
        if (yArg is not IntValueType y) return new();

        _virtualEnvironment.InputState.MouseState.XPos = x.Value;
        _virtualEnvironment.InputState.MouseState.YPos = y.Value;

        return new();
    }
}