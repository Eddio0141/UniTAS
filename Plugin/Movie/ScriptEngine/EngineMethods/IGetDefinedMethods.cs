using System.Collections.Generic;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods;

public interface IGetDefinedMethods
{
    IEnumerable<EngineExternalMethodBase> GetExternMethods();
}