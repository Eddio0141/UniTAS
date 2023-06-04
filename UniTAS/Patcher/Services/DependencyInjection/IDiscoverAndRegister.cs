using StructureMap;

namespace UniTAS.Patcher.Services.DependencyInjection;

public interface IDiscoverAndRegister
{
    void Register<TAssemblyContainingType>(ConfigurationExpression config);
}