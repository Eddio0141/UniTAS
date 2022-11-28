using System.Collections.Generic;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods;

public class EngineExternalMethods : IGetDefinedMethods
{
    private readonly EngineExternalMethodBase[] _externMethods = new[]
    {
        new PrintExternalMethod()
    };

    public IEnumerable<EngineExternalMethodBase> GetExternMethods()
    {
        return _externMethods;
    }
}