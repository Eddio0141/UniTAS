using NinjectWrap.Modules;
using UniTASPlugin.GameEnvironment.Interfaces;

namespace UniTASPlugin.NInjectModules;

public class GameEnvironmentModule : NinjectModule
{
    public override void Load()
    {
        Bind<IRunVirtualEnvironmentProperty>().To<GameEnvironment.VirtualEnvironment>();
        Bind<IInputStateProperty>().To<GameEnvironment.VirtualEnvironment>();

        Bind<GameEnvironment.VirtualEnvironment>().ToSelf().InSingletonScope();
    }
}