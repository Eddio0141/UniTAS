using StructureMap;

namespace UniTAS.Plugin.Services;

public interface IDiscoverAndRegister
{
    void Register<TAssemblyContainingType>(ConfigurationExpression config);
}