using Ninject.Activation;

namespace UniTASPlugin.GameEnvironment;

// ReSharper disable once ClassNeverInstantiated.Global
public class VirtualEnvironmentProvider : Provider<VirtualEnvironment>
{
    private readonly VirtualEnvironment _virtualEnvironment = new();

    protected override VirtualEnvironment CreateInstance(IContext context)
    {
        return _virtualEnvironment;
    }
}