using System.Collections.Generic;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ValueTypes;

namespace UniTASPlugin.Movie.EngineMethods;

public class ClearHeldKeysExternalMethod : EngineExternalMethod
{
    private readonly IVirtualEnvironmentFactory _virtualEnvironmentFactory;

    public ClearHeldKeysExternalMethod(IVirtualEnvironmentFactory virtualEnvironmentFactory) : base("clear_held_keys")
    {
        _virtualEnvironmentFactory = virtualEnvironmentFactory;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, MovieRunner runner)
    {
        _virtualEnvironmentFactory.GetVirtualEnv().InputState.KeyboardState.Keys.Clear();
        return new();
    }
}