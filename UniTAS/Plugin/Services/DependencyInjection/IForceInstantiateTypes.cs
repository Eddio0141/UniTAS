namespace UniTAS.Plugin.Services.DependencyInjection;

public interface IForceInstantiateTypes
{
    void InstantiateTypes<TAssemblyContainingType>();
}