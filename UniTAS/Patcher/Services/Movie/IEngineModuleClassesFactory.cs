using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.Movie;

namespace UniTAS.Patcher.Services.Movie;

public interface IEngineModuleClassesFactory
{
    IEnumerable<EngineMethodClass> GetAll(IMovieEngine engine);
}