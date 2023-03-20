using System.Collections.Generic;
using UniTAS.Plugin.Interfaces.Movie;

namespace UniTAS.Plugin.Services.Movie;

public interface IEngineModuleClassesFactory
{
    IEnumerable<EngineMethodClass> GetAll(IMovieEngine engine);
}