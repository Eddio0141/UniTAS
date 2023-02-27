using System.Collections.Generic;
using StructureMap;
using StructureMap.Pipeline;
using UniTAS.Plugin.Movie.Engine;

namespace UniTAS.Plugin.Movie.EngineMethods;

public class EngineMethodClassesFactory : IEngineMethodClassesFactory
{
    private readonly IContainer _container;

    public EngineMethodClassesFactory(IContainer container)
    {
        _container = container;
    }

    public IEnumerable<EngineMethodClass> GetAll(IMovieEngine engine)
    {
        var args = new ExplicitArguments();
        args.Set(engine);

        // pass script to each EngineMethodClass
        return _container.GetAllInstances<EngineMethodClass>(args);
    }
}