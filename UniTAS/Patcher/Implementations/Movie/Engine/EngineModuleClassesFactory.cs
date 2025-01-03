using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Movie;
using UniTAS.Patcher.Services.Movie;
using UniTAS.Patcher.Utils;

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
        // pass script to each EngineMethodClass
        return _container.GetAllInstances<EngineMethodClass>(new ConstructorArg(nameof(engine), engine));
    }
}