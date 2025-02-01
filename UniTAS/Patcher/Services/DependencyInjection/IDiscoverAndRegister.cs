using StructureMap;
using UniTAS.Patcher.Models.DependencyInjection;

namespace UniTAS.Patcher.Services.DependencyInjection;

public interface IDiscoverAndRegister
{
    void Register<TAssemblyContainingType>(ConfigurationExpression config);
}