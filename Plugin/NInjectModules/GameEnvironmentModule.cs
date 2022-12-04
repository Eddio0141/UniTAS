using Ninject.Modules;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameEnvironment.Interfaces;

namespace UniTASPlugin.NInjectModules;

public class GameEnvironmentModule : NinjectModule
{
    public override void Load()
    {
        Bind<VirtualEnvironment>().ToSelf().InSingletonScope();
    }
}