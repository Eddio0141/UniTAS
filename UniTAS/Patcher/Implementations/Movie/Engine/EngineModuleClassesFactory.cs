using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StructureMap;
using StructureMap.Pipeline;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Movie;
using UniTAS.Patcher.Services.Movie;

namespace UniTAS.Patcher.Implementations.Movie.Engine;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[Register]
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