using System.Collections.Generic;
using MoonSharp.Interpreter;
using UniTAS.Plugin.Movie.Engine;

namespace UniTAS.Plugin.Movie.EngineMethods;

public interface IEngineMethodClassesFactory
{
    IEnumerable<EngineMethodClass> GetAll(IMovieEngine engine);
}