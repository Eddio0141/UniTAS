using Ninject.Modules;
using UniTASPlugin.GameEnvironment;

namespace UniTASPlugin.NInjectModules;

public class GameEnvironmentModule : NinjectModule
{
    public override void Load()
    {
        Bind<IVirtualEnvironmentService>().ToProvider<VirtualEnvironmentProvider>();
    }
}