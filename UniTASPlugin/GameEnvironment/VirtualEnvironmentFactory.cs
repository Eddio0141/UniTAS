using StructureMap;

namespace UniTASPlugin.GameEnvironment;

public class VirtualEnvironmentFactory : IVirtualEnvironmentFactory
{
    private readonly IContainer _container;

    public VirtualEnvironmentFactory(IContainer container)
    {
        _container = container;
    }

    public VirtualEnvironment GetVirtualEnv()
    {
        return _container.GetInstance<VirtualEnvironment>();
    }
}