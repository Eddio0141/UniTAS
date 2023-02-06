using System.Collections.Generic;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.EngineMethods;

public class ClearHeldButtonsExternalMethod : EngineExternalMethod
{
    private readonly VirtualEnvironment _virtualEnvironment;

    public ClearHeldButtonsExternalMethod(VirtualEnvironment virtualEnvironmentFactory) : base(
        "clear_held_buttons")
    {
        _virtualEnvironment = virtualEnvironmentFactory;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
    {
        _virtualEnvironment.InputState.KeyboardState.Keys.Clear();
        return new();
    }
}