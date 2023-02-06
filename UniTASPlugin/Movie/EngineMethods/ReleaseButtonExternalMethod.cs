using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.EngineMethods;

public class ReleaseButtonExternalMethod : EngineExternalMethod
{
    private readonly IVirtualEnvironmentFactory _virtualEnvironmentFactory;

    public ReleaseButtonExternalMethod(IVirtualEnvironmentFactory virtualEnvironmentFactory) : base("release_button", 1)
    {
        _virtualEnvironmentFactory = virtualEnvironmentFactory;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
    {
        var buttonName = args.First().First();
        if (buttonName is not StringValueType buttonNameString) return new();

        _virtualEnvironmentFactory.GetVirtualEnv().InputState.ButtonState.Buttons.Remove(buttonNameString.Value);

        return new();
    }
}