using UniTAS.Patcher.Models.DependencyInjection;

namespace UniTAS.Patcher.Services.DependencyInjection;

public interface IForceInstantiateTypes
{
    void InstantiateTypes<TAssemblyContainingType>(RegisterTiming timing);
}