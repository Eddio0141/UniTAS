using Ninject.Modules;
using UniTASPlugin.GameEnvironment.Interfaces;

namespace UniTASPlugin.NInjectModules;

public class GameEnvironmentModule : NinjectModule
{
    public override void Load()
    {
        Bind<IRunVirtualEnvironmentProperty>().To<GameEnvironment.GameEnvironment>();
        Bind<IInputStateProperty>().To<GameEnvironment.GameEnvironment>();

        Bind<GameEnvironment.GameEnvironment>().ToSelf().InSingletonScope();
    }
}