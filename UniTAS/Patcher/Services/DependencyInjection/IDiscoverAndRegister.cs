using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Services.DependencyInjection;

public interface IDiscoverAndRegister
{
    void Register<TAssemblyContainingType>(IContainer container, RegisterTiming timing);
}