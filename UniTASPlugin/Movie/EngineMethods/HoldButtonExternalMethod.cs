using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.EngineMethods;

public class HoldButtonExternalMethod : EngineExternalMethod
{
    private readonly VirtualEnvironment _virtualEnvironment;

    public HoldButtonExternalMethod(VirtualEnvironment virtualEnvironmentFactory) : base("hold_button", 1)
    {
        _virtualEnvironment = virtualEnvironmentFactory;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
    {
        var buttonName = args.First().First();
        if (buttonName is not StringValueType buttonNameString) return new();

        _virtualEnvironment.InputState.ButtonState.Buttons.Add(buttonNameString.Value);

        return new();
    }
}