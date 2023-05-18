using StructureMap;

namespace UniTAS.Plugin.Services.DependencyInjection;

public interface IDiscoverAndRegister
{
    void Register<TAssemblyContainingType>(ConfigurationExpression config);
}