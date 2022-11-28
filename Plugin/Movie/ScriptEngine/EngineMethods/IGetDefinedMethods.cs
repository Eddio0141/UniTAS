using System.Collections.Generic;
using UniTASPlugin.Movie.ScriptEngine.Models.ExternalMethods;

namespace UniTASPlugin.Movie.ScriptEngine;

public interface IGetDefinedMethods
{
    ICollection<EngineExternalMethodBase> GetExternMethods();
}