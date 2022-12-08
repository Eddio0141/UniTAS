using Ninject.Extensions.Factory;
using Ninject.Modules;
using UniTASPlugin.GameEnvironment;

namespace UniTASPlugin.NInjectModules;

public class GameEnvironmentModule : NinjectModule
{
    public override void Load()
    {
        Bind<VirtualEnvironment>().ToSelf().InSingletonScope();
        Bind<IVirtualEnvironmentService>().ToFactory();
    }
}