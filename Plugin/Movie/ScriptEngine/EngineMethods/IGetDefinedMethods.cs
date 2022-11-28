using System.Collections.Generic;

namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods;

public interface IGetDefinedMethods
{
    ICollection<EngineExternalMethodBase> GetExternMethods();
}