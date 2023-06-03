namespace UniTAS.Patcher.Services.DependencyInjection;

public interface IForceInstantiateTypes
{
    void InstantiateTypes<TAssemblyContainingType>();
}