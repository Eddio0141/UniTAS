using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StructureMap;
using StructureMap.Pipeline;
using UniTAS.Plugin.Interfaces.Movie;
using UniTAS.Plugin.Services.Movie;

namespace UniTAS.Plugin.Implementations.Movie.Engine;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class EngineModuleClassesFactory : IEngineModuleClassesFactory
{
    private readonly IContainer _container;

    public EngineModuleClassesFactory(IContainer container)
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