using System.Collections.Generic;
using UniTAS.Plugin.Movie.Engine;

namespace UniTAS.Plugin.Movie.EngineMethods;

public interface IEngineMethodClassesFactory
{
    IEnumerable<EngineMethodClass> GetAll(IMovieEngine engine);
}