using System.Collections.Generic;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods.Exceptions;

public class ClearHeldKeysExternalMethod : EngineExternalMethod
{
    private readonly IVirtualEnvironmentService _virtualEnvironmentService;

    public ClearHeldKeysExternalMethod(IVirtualEnvironmentService virtualEnvironmentService) : base("clear_held_keys")
    {
        _virtualEnvironmentService = virtualEnvironmentService;
    }

    public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, ScriptEngineMovieRunner runner)
    {
        _virtualEnvironmentService.GetVirtualEnv().InputState.KeyboardState.Keys.Clear();
        return new();
    }
}