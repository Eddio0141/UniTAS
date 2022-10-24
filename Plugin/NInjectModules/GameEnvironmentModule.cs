using Ninject.Modules;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameEnvironment.Interfaces;

namespace UniTASPlugin.NInjectModules;

public class GameEnvironmentModule : NinjectModule
{
    public override void Load()
    {
        Bind<IRunVirtualEnvironmentProperty>().To<VirtualEnvironment>();
        Bind<IInputStateProperty>().To<VirtualEnvironment>();

        Bind<VirtualEnvironment>().ToSelf().InSingletonScope();
    }
}