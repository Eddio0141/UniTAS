using System.Collections.Generic;
using UniTASPlugin.Movie.ScriptEngine.Models.ExternalMethods;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods;

public class EngineExternalMethods : IGetDefinedMethods
{
    private readonly EngineExternalMethodBase[] _externMethods = new[]
    {
        new PrintExternalMethodBase()
    };

    public ICollection<EngineExternalMethodBase> GetExternMethods()
    {
        return _externMethods;
    }
}