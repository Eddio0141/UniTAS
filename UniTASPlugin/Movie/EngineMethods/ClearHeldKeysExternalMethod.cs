using System.Collections.Generic;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.EngineMethods;

public class ClearHeldKeysExternalMethod : EngineExternalMethod
{
    private readonly VirtualEnvironment _virtualEnvironment;

    public ClearHeldKeysExternalMethod(VirtualEnvironment virtualEnvironmentFactory) : base("clear_held_keys")
    {
        _virtualEnvironment = virtualEnvironmentFactory;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
    {
        _virtualEnvironment.InputState.KeyboardState.Keys.Clear();
        return new();
    }
}